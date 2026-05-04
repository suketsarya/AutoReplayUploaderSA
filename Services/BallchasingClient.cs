using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace SuketAutoReplayUploader.Services;

public class BallchasingClient : IBallchasingClient
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private string _visibility = "public";
    private Dictionary<string, string> _maps = new();

    public enum ApiStatus { Unknown, Checking, Valid, Invalid, ServerError }
    public record AccountInfo(string Name, string Type, QuotaInfo Quota);
    public record QuotaInfo(QuotaDetails Day, QuotaDetails Week);
    public record QuotaDetails(int Max, int Used, bool IsUnlimited = false);

    public event Action<AccountInfo>? AccountInfoUpdated;

    public BallchasingClient()
    {
        _httpClient = new HttpClient();
    }

    public void Configure(string apiKey, string visibility)
    {
        _apiKey = apiKey?.Trim();
        _visibility = visibility;
    }

    public async Task<(bool Success, string Message, string? Location)> UploadReplayAsync(string filePath, string? visibility = null, Action<int>? onRetry = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return (false, "API Key not configured.", null);

        if (!File.Exists(filePath))
            return (false, "File does not exist.", null);

        try
        {
            var activeVisibility = visibility ?? _visibility;
            var url = $"https://ballchasing.com/api/v2/upload?visibility={activeVisibility.ToLower()}";
            
            int retryCount = 0;
            int maxRetries = 3;
            int backoffSeconds = 3;

            while (true)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.TryAddWithoutValidation("Authorization", _apiKey);
                
                // Content must be recreated for each retry
                using var currentForm = new MultipartFormDataContent();
                using var currentFileContent = new StreamContent(File.OpenRead(filePath));
                currentFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                currentForm.Add(currentFileContent, "file", Path.GetFileName(filePath));
                request.Content = currentForm;

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    string? location = null;
                    try 
                    {
                        using var doc = JsonDocument.Parse(body);
                        location = doc.RootElement.GetProperty("location").GetString();
                    } catch { }
                    return (true, "Upload successful.", location);
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

    public async Task<ApiStatus> CheckApiStatusAsync(string? apiKey, Action<int>? onRetry = null)
    {
        apiKey = apiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
            return ApiStatus.Invalid;

        int retryCount = 0;
        int maxRetries = 3;
        int backoffSeconds = 2;

        while (true)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://ballchasing.com/api/");
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        
                        string name = root.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown";
                        string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "regular" : "regular";
                        
                        QuotaInfo? quotaInfo = null;
                        if (root.TryGetProperty("quota", out var quota))
                        {
                            QuotaDetails GetDetails(JsonElement element, string key)
                            {
                                if (element.TryGetProperty(key, out var details))
                                {
                                    bool isUnlimited = details.TryGetProperty("unlimited", out var u) && u.GetBoolean();
                                    int max = details.TryGetProperty("max", out var m) ? m.GetInt32() : 0;
                                    int used = details.TryGetProperty("used", out var usd) ? usd.GetInt32() : 0;
                                    return new QuotaDetails(max, used, isUnlimited);
                                }
                                return new QuotaDetails(0, 0);
                            }

                            quotaInfo = new QuotaInfo(
                                GetDetails(quota, "uploads_in_24h"),
                                GetDetails(quota, "uploads_in_7d")
                            );
                        }
                        else
                        {
                            quotaInfo = new QuotaInfo(new QuotaDetails(0, 0), new QuotaDetails(0, 0));
                        }

                        var accountInfo = new AccountInfo(name, type, quotaInfo);
                        AccountInfoUpdated?.Invoke(accountInfo);
                    }
                    catch { }
                    return ApiStatus.Valid;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ApiStatus.Invalid;
                }
                else if (response.StatusCode == (System.Net.HttpStatusCode)429) // Too Many Requests
                {
                    return ApiStatus.ServerError; // Treat as server error to trigger retries if appropriate
                }
                else if ((int)response.StatusCode >= 500)
                {
                    if (retryCount < maxRetries)
                    {
                        retryCount++;
                        onRetry?.Invoke(retryCount);
                        await Task.Delay(backoffSeconds * 1000);
                        backoffSeconds *= 2;
                        continue;
                    }
                    return ApiStatus.ServerError;
                }
                else
                {
                    return ApiStatus.Invalid;
                }
            }
            catch
            {
                if (retryCount < maxRetries)
                {
                    retryCount++;
                    onRetry?.Invoke(retryCount);
                    await Task.Delay(backoffSeconds * 1000);
                    backoffSeconds *= 2;
                    continue;
                }
                return ApiStatus.ServerError;
            }
        }
    }
}
