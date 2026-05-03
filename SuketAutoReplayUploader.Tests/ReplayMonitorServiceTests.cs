using SuketAutoReplayUploader.Models;
using SuketAutoReplayUploader.Services;
using System.IO;
using Xunit;

namespace SuketAutoReplayUploader.Tests;

public class ReplayMonitorServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly ReplayMonitorService _service;

    public ReplayMonitorServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "SuketUploaderTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        
        var settingsFile = Path.Combine(_testDir, "settings.json");
        var settings = new SettingsService(settingsFile);
        _service = new ReplayMonitorService(settings);
    }

    [Fact]
    public async Task StartMonitoring_DiscoversExistingFiles()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test1.replay");
        File.WriteAllText(filePath, "dummy content");

        // Act
        _service.StartMonitoring(_testDir, 1);

        // Assert
        Assert.Single(_service.Replays);
        Assert.Equal("test1.replay", _service.Replays[0].FileName);
    }

    [Fact]
    public async Task NewFile_IsDiscoveredByWatcher()
    {
        // Arrange
        _service.StartMonitoring(_testDir, 1);
        var filePath = Path.Combine(_testDir, "new.replay");

        // Act
        File.WriteAllText(filePath, "dummy content");
        
        // Wait for FileSystemWatcher to fire (it can be slow)
        await Task.Delay(500);

        // Assert
        Assert.Contains(_service.Replays, r => r.FileName == "new.replay");
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}
