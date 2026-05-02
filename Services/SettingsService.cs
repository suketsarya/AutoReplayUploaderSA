using System.IO;
using System.Text.Json;

namespace SuketAutoReplayUploader.Services;

public class AppSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Visibility { get; set; } = "public";
    public string ReplayFolder { get; set; } = string.Empty;
    public int PollingInterval { get; set; } = 5;
    public bool AutoUpload { get; set; } = false;
    public List<string> HiddenReplays { get; set; } = new();
}

public class SettingsService
{
    private readonly string _filePath;
    public AppSettings Current { get; private set; }

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "SuketAutoReplayUploader");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");

        Current = Load();
        
        // Default replay folder if not set
        if (string.IsNullOrEmpty(Current.ReplayFolder))
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var rlDemos = Path.Combine(documents, "My Games", "Rocket League", "TAGame", "Demos");
            if (Directory.Exists(rlDemos))
            {
                Current.ReplayFolder = rlDemos;
            }
        }
    }

    private AppSettings Load()
    {
        if (!File.Exists(_filePath)) return new AppSettings();
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
