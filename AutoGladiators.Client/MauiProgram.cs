using AutoGladiators.Core.Services.Logging;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using AutoGladiators.Core.Services.Storage;
using AutoGladiators.Client.Services.Storage;
using AutoGladiators.Core.Services;

namespace AutoGladiators.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            // builder.Logging.AddConsole(); // Remove or comment out this line; not supported on MAUI/Android by default
#endif

            // Register DI services here as needed
            // builder.Services.AddSingleton<GameStateService>();

            // Register services before building the app
            builder.Services.AddSingleton<IAppStorage, AppStorage>();
            builder.Services.AddSingleton<NPCDialogueService>();
            builder.Services.AddSingleton<NPCDialogueLoader>();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<GameStateService>();

            var app = builder.Build();

            try
            {
                // Wire AppLog to platform logger factory
                var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
                AppLog.Initialize(loggerFactory);
                
                // Test logging to ensure it's working
                var logger = AppLog.For("MauiProgram");
                logger.Info("AutoGladiators MAUI app initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize AppLog: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Continue without custom logging - app should still work
            }

            return app;
        }
    }
}
