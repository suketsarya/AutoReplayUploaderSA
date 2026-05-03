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
            AppDomain.CurrentDomain.UnhandledException += (s, e) => 
                ShowError(e.ExceptionObject as Exception, "AppDomain Unhandled Exception");
            
            DispatcherUnhandledException += (s, e) => 
            {
                ShowError(e.Exception, "Dispatcher Unhandled Exception");
                e.Handled = true;
            };

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            
            // Register our services
            serviceCollection.AddSingleton<SettingsService>();
            serviceCollection.AddSingleton<IBallchasingClient, BallchasingClient>();
            serviceCollection.AddSingleton<IReplayMonitorService, ReplayMonitorService>();

            Services = serviceCollection.BuildServiceProvider();
        }

        private void ShowError(Exception? ex, string title)
        {
            var message = ex?.ToString() ?? "Unknown error";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
