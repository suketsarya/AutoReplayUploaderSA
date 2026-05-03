using SuketAutoReplayUploader.Services;
using System.IO;
using Xunit;

namespace SuketAutoReplayUploader.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testFile;

    public SettingsServiceTests()
    {
        _testFile = Path.Combine(Path.GetTempPath(), "settings_" + Guid.NewGuid() + ".json");
    }

    [Fact]
    public void DefaultSettings_AreCorrect()
    {
        // Arrange & Act
        var service = new SettingsService(_testFile);

        // Assert
        Assert.NotNull(service.Current);
        Assert.Equal("public", service.Current.Visibility);
        Assert.Equal(5, service.Current.PollingInterval);
        Assert.False(service.Current.AutoUpload);
    }

    [Fact]
    public void SaveAndLoad_PersistsSettings()
    {
        // Arrange
        var service = new SettingsService(_testFile);
        var testApiKey = "test-api-key-" + Guid.NewGuid();
        service.Current.ApiKey = testApiKey;
        service.Current.Visibility = "private";

        // Act
        service.Save();
        var service2 = new SettingsService(_testFile);

        // Assert
        Assert.Equal(testApiKey, service2.Current.ApiKey);
        Assert.Equal("private", service2.Current.Visibility);
    }

    public void Dispose()
    {
        if (File.Exists(_testFile)) File.Delete(_testFile);
    }
}
