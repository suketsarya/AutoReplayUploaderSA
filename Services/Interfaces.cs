using SuketAutoReplayUploader.Models;

namespace SuketAutoReplayUploader.Services;

public interface IBallchasingClient
{
    event Action<BallchasingClient.AccountInfo>? AccountInfoUpdated;
    void Configure(string apiKey, string visibility);
    Task<(bool Success, string Message, string? Location)> UploadReplayAsync(string filePath, string? visibility = null, Action<int>? onRetry = null);
    Task<BallchasingClient.ApiStatus> CheckApiStatusAsync(string? apiKey, Action<int>? onRetry = null);
}

public interface IReplayMonitorService : IDisposable
{
    System.Collections.ObjectModel.ObservableCollection<ReplayFile> Replays { get; }
    event Action? ReplaysChanged;
    event Action<ReplayFile>? OnNewReplayDiscovered;
    void StartMonitoring(string path, int intervalMinutes, DateTime? baselineDate = null);
    void ForceAddReplay(string fullPath);
    void Save();
}
