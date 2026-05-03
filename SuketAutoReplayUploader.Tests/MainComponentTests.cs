using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SuketAutoReplayUploader.Components;
using SuketAutoReplayUploader.Services;
using SuketAutoReplayUploader.Models;
using System.Collections.ObjectModel;
using Xunit;

namespace SuketAutoReplayUploader.Tests;

public class MainComponentTests : TestContext
{
    [Fact]
    public void MainComponent_RendersTitle()
    {
        // Arrange
        var mockMonitor = new Mock<IReplayMonitorService>();
        mockMonitor.Setup(m => m.Replays).Returns(new ObservableCollection<ReplayFile>());
        
        var mockClient = new Mock<IBallchasingClient>();
        var settingsService = new SettingsService();

        Services.AddSingleton(mockMonitor.Object);
        Services.AddSingleton(mockClient.Object);
        Services.AddSingleton(settingsService);

        // Act
        var cut = RenderComponent<Main>();

        // Assert
        cut.Find(".title").MarkupMatches("<div class=\"title\">AutoReplayUploaderSA</div>");
    }

    [Fact]
    public void MainComponent_ShowsEmptyListMessage()
    {
        // Arrange
        var mockMonitor = new Mock<IReplayMonitorService>();
        mockMonitor.Setup(m => m.Replays).Returns(new ObservableCollection<ReplayFile>());
        
        var mockClient = new Mock<IBallchasingClient>();
        var settingsService = new SettingsService();

        Services.AddSingleton(mockMonitor.Object);
        Services.AddSingleton(mockClient.Object);
        Services.AddSingleton(settingsService);

        // Act
        var cut = RenderComponent<Main>();

        // Assert
        var replayList = cut.Find(".replay-list");
        Assert.Empty(replayList.Children);
    }
}
