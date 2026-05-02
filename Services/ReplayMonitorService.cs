using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using SuketAutoReplayUploader.Models;

namespace SuketAutoReplayUploader.Services;

public class ReplayMonitorService : IDisposable
{
    private FileSystemWatcher? _watcher;
    private string _watchPath = string.Empty;
    private readonly ObservableCollection<ReplayFile> _replays = new();
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public ObservableCollection<ReplayFile> Replays => _replays;
    public event Action? ReplaysChanged;
    public event Action<ReplayFile>? OnNewReplayDiscovered;

    public void StartMonitoring(string path, int intervalMinutes)
    {
        _watchPath = path;
        
        if (!Directory.Exists(path)) return;

        // Initial scan
        ScanFolder();

        // Setup Watcher
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(path, "*.replay")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };
        _watcher.Created += (s, e) => AddReplay(e.FullPath);

        // Setup Polling Timer
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        _ = PollAsync(_cts.Token);
    }

    private void ScanFolder()
    {
        if (!Directory.Exists(_watchPath)) return;

        var files = Directory.GetFiles(_watchPath, "*.replay");
        foreach (var file in files)
        {
            AddReplay(file);
        }
    }

    private void AddReplay(string fullPath)
    {
        if (_replays.Any(r => r.FullPath == fullPath)) return;

        var info = new FileInfo(fullPath);
        var replay = new ReplayFile
        {
            FileName = info.Name,
            FullPath = fullPath,
            DateCreated = info.CreationTime,
            Status = UploadStatus.Pending
        };

        // UI thread safety might be needed depending on how this is consumed
        App.Current?.Dispatcher.Invoke(() => 
        {
            _replays.Add(replay);
            ReplaysChanged?.Invoke();
            OnNewReplayDiscovered?.Invoke(replay);
        });
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
