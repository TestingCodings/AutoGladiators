using AutoGladiators.Client.Services.Logging;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

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

            var app = builder.Build();

            // Wire AppLog to platform logger factory
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            AppLog.Initialize(loggerFactory);

            return app;
        }
    }
}
