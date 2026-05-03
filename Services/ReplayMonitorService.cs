using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using SuketAutoReplayUploader.Models;

namespace SuketAutoReplayUploader.Services;

public class ReplayMonitorService : IReplayMonitorService
{
    private FileSystemWatcher? _watcher;
    private string _watchPath = string.Empty;
    private readonly ObservableCollection<ReplayFile> _replays = new();
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private DateTime? _baselineDate;
    private readonly HashSet<string> _forceVisibleFiles = new();
    private readonly SettingsService _settings;
    private readonly string _persistencePath;

    public ReplayMonitorService(SettingsService settings)
    {
        _settings = settings;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "SuketAutoReplayUploader");
        _persistencePath = Path.Combine(appFolder, "replays.json");

        _replays.CollectionChanged += (s, e) => SavePersistedReplays();
    }

    public ObservableCollection<ReplayFile> Replays => _replays;
    public event Action? ReplaysChanged;
    public event Action<ReplayFile>? OnNewReplayDiscovered;

    public void StartMonitoring(string path, int intervalMinutes, DateTime? baselineDate = null)
    {
        _watchPath = path;
        _baselineDate = baselineDate;
        
        if (!Directory.Exists(path)) return;

        LoadPersistedReplays();
        ScanFolder(isInitial: true);

        // Setup Watcher
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(path, "*.replay")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };
        _watcher.Created += (s, e) => AddReplay(e.FullPath);
        _watcher.Deleted += (s, e) => RemoveReplay(e.FullPath);
        _watcher.Renamed += (s, e) => { RemoveReplay(e.OldFullPath); AddReplay(e.FullPath); };

        // Setup Polling Timer
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        _ = PollAsync(_cts.Token);
    }

    private void ScanFolder(bool isInitial = false)
    {
        if (!Directory.Exists(_watchPath)) return;

        var files = Directory.GetFiles(_watchPath, "*.replay").ToHashSet();
        
        Action action = () => 
        {
            // Remove replays from list if they no longer exist on disk
            var toRemove = _replays.Where(r => !files.Contains(r.FullPath)).ToList();
            bool changed = false;
            foreach (var r in toRemove)
            {
                _replays.Remove(r);
                changed = true;
            }

            foreach (var file in files)
            {
                if (_settings.Current.ExcludedFilePaths.Contains(file)) continue;

                if (!_replays.Any(r => r.FullPath == file))
                {
                    var info = new FileInfo(file);
                    if (!_forceVisibleFiles.Contains(file) && _baselineDate.HasValue && info.CreationTime <= _baselineDate.Value)
                    {
                        continue;
                    }

                    InternalAddReplay(file);
                    changed = true;
                }
            }

            if (changed) 
            {
                SavePersistedReplays();
                ReplaysChanged?.Invoke();
            }
        };

        if (App.Current?.Dispatcher != null)
            App.Current.Dispatcher.Invoke(action);
        else
            action();
    }

    private void RemoveReplay(string fullPath)
    {
        Action action = () => 
        {
            var replay = _replays.FirstOrDefault(r => r.FullPath == fullPath);
            if (replay != null)
            {
                _replays.Remove(replay);
                SavePersistedReplays();
                ReplaysChanged?.Invoke();
            }
        };

        if (App.Current?.Dispatcher != null)
            App.Current.Dispatcher.Invoke(action);
        else
            action();
    }

    private void AddReplay(string fullPath)
    {
        var info = new FileInfo(fullPath);
        if (!_forceVisibleFiles.Contains(fullPath) && _baselineDate.HasValue && info.CreationTime <= _baselineDate.Value) return;

        Action action = () => 
        {
            if (_settings.Current.ExcludedFilePaths.Contains(fullPath)) return;
            if (_replays.Any(r => r.FullPath == fullPath)) return;
            InternalAddReplay(fullPath);
            SavePersistedReplays();
            ReplaysChanged?.Invoke();
        };

        if (App.Current?.Dispatcher != null)
            App.Current.Dispatcher.Invoke(action);
        else
            action();
    }

    public void ForceAddReplay(string fullPath)
    {
        if (_settings.Current.ExcludedFilePaths.Remove(fullPath))
        {
            _settings.Save();
        }
        _forceVisibleFiles.Add(fullPath);
        AddReplay(fullPath);
    }

    private void InternalAddReplay(string fullPath)
    {
        if (!File.Exists(fullPath)) return;
        var info = new FileInfo(fullPath);
        var replay = new ReplayFile
        {
            FileName = info.Name,
            FullPath = fullPath,
            DateCreated = info.CreationTime,
            Status = UploadStatus.Pending
        };
        _replays.Add(replay);
        SavePersistedReplays();
        OnNewReplayDiscovered?.Invoke(replay);
    }

    private void LoadPersistedReplays()
    {
        if (!File.Exists(_persistencePath)) return;
        try
        {
            var json = File.ReadAllText(_persistencePath);
            var loaded = System.Text.Json.JsonSerializer.Deserialize<List<ReplayFile>>(json);
            if (loaded != null)
            {
                foreach (var r in loaded)
                {
                    if (File.Exists(r.FullPath) && !_replays.Any(x => x.FullPath == r.FullPath))
                    {
                        _replays.Add(r);
                    }
                }
            }
        }
        catch { }
    }

    public void Save() => SavePersistedReplays();

    public void SavePersistedReplays()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_replays.ToList(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_persistencePath, json);
        }
        catch { }
    }

    private async Task PollAsync(CancellationToken token)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(token))
            {
                ScanFolder();
            }
        }
        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
        _timer?.Dispose();
    }
}
