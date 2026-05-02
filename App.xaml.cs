using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SuketAutoReplayUploader.Services;

namespace SuketAutoReplayUploader
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            
            // Register our services
            serviceCollection.AddSingleton<SettingsService>();
            serviceCollection.AddSingleton<BallchasingClient>();
            serviceCollection.AddSingleton<ReplayMonitorService>();

            Services = serviceCollection.BuildServiceProvider();
        }
    }
}
