using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace SuketAutoReplayUploader.Services;

public class BallchasingClient
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private string _visibility = "public";
    private Dictionary<string, string> _maps = new();

    public event Action<RateLimitInfo>? RateLimitUpdated;

    public BallchasingClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SuketAutoReplayUploader/1.0");
        _ = FetchMapsAsync();
    }

    private async Task FetchMapsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://ballchasing.com/api/maps");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var maps = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (maps != null) _maps = maps;
            }
        }
        catch { }
    }

    public string GetMapName(string code) => _maps.TryGetValue(code, out var name) ? name : code;

    public void Configure(string apiKey, string visibility)
    {
        _apiKey = apiKey;
        _visibility = visibility;
    }

    public async Task<(bool Success, string Message, string? Location)> UploadReplayAsync(string filePath, Action<int>? onRetry = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return (false, "API Key not configured.", null);

        if (!File.Exists(filePath))
            return (false, "File does not exist.", null);

        try
        {
            var url = $"https://ballchasing.com/api/v2/upload?visibility={_visibility.ToLower()}";
            
            int retryCount = 0;
            int maxRetries = 3;
            int backoffSeconds = 3;

            while (true)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue(_apiKey);
                
                // Content must be recreated for each retry
                using var currentForm = new MultipartFormDataContent();
                using var currentFileContent = new StreamContent(File.OpenRead(filePath));
                currentFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                currentForm.Add(currentFileContent, "file", Path.GetFileName(filePath));
                request.Content = currentForm;

                var response = await _httpClient.SendAsync(request);
                UpdateRateLimits(response.Headers);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Upload successful.", null);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    string? location = null;
                    try 
                    {
                        using var doc = JsonDocument.Parse(body);
                        location = doc.RootElement.GetProperty("location").GetString();
                    } catch { }
                    return (false, "Replay already exists.", location);
                }
                else if ((int)response.StatusCode >= 500 && retryCount < maxRetries)
                {
                    retryCount++;
                    onRetry?.Invoke(retryCount);
                    await Task.Delay(backoffSeconds * 1000);
                    backoffSeconds *= 2; // Exponential backoff
                    continue;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return (false, $"Upload failed ({response.StatusCode}): {errorBody}", null);
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }

    private void UpdateRateLimits(HttpResponseHeaders headers)
    {
        var info = new RateLimitInfo();

        if (headers.TryGetValues("X-UploadLimit-Remaining-Day", out var dayRem))
            info.RemainingDay = int.Parse(dayRem.First());
        if (headers.TryGetValues("X-UploadLimit-Remaining-Week", out var weekRem))
            info.RemainingWeek = int.Parse(weekRem.First());

        RateLimitUpdated?.Invoke(info);
    }
}

public class RateLimitInfo
{
    public int RemainingDay { get; set; } = -1;
    public int RemainingWeek { get; set; } = -1;
}
