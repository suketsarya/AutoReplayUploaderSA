using Microsoft.Extensions.DependencyInjection;
using SuketAutoReplayUploader.Services;
using Xunit;

namespace SuketAutoReplayUploader.Tests;

public class SmokeTests
{
    [Fact]
    public void DependencyInjection_CanResolveAllServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddSingleton<IBallchasingClient, BallchasingClient>();
        serviceCollection.AddSingleton<IReplayMonitorService, ReplayMonitorService>();

        // Act
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetRequiredService<SettingsService>());
        Assert.NotNull(provider.GetRequiredService<IBallchasingClient>());
        Assert.NotNull(provider.GetRequiredService<IReplayMonitorService>());
    }
}
