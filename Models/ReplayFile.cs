namespace SuketAutoReplayUploader.Models;

public enum UploadStatus
{
    Pending,
    Uploading,
    Success,
    Failed,
    Excluded,
    Retrying,
    Conflict
}

public class ReplayFile
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public UploadStatus Status { get; set; } = UploadStatus.Pending;
    public string Message { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public DateTime? UploadTime { get; set; }
    public string? ConflictUrl { get; set; }
    public string? OnlineUrl { get; set; }
}
