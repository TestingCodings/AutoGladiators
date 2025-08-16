using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
            // Optionally add console logging for Windows:
            // builder.Logging.AddConsole();
#endif

            // Register DI services here
            // builder.Services.AddSingleton<GameStateService>();

            var app = builder.Build();

            // Initialize the central AppLog so all classes using AppLog.For<T>() work
            var loggerFactory = app.Services.GetRequiredService<IAppLoggerFactory>();
            AppLog.Initialize(loggerFactory);

            return app;
        }
    }
}
// This file is the entry point for the MAUI application.
// It sets up the application, configures fonts, and initializes logging.
// It also allows for dependency injection of services like GameStateService.
// The logging configuration can be adjusted based on the environment (e.g., DEBUG).