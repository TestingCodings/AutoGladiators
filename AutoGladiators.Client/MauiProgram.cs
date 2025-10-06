using AutoGladiators.Core.Services.Logging;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using AutoGladiators.Core.Services.Storage;
using AutoGladiators.Client.Services.Storage;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Exploration;
using AutoGladiators.Core.Rng;
using AutoGladiators.Client.Pages;

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
            builder.Services.AddSingleton<PlayerProfileService>();
            
            // Register Core Dependencies
            builder.Services.AddSingleton<IRng, DefaultRng>();
            
            // Register Exploration Services (Pokemon-style exploration system)
            builder.Services.AddSingleton<WorldManager>();
            builder.Services.AddSingleton<MovementManager>();
            builder.Services.AddSingleton<AutoGladiators.Core.Services.Exploration.EncounterService>();
            
            // Register Pages for dependency injection
            builder.Services.AddTransient<ExplorationPage>();
            builder.Services.AddTransient<AdventurePage>();
            builder.Services.AddTransient<BotRosterPage>();
            builder.Services.AddTransient<InventoryPage>();
            
            // Register Visual Asset Services (Sprint 4)
            builder.Services.AddSingleton<AutoGladiators.Client.Services.SpriteManager>();
            builder.Services.AddSingleton<AutoGladiators.Client.Services.AnimationManager>();

            var app = builder.Build();

            try
            {
                // Wire AppLog to platform logger factory
                var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
                AppLog.Initialize(loggerFactory);
                
                // Test logging to ensure it's working
                var logger = AppLog.For("MauiProgram");
                logger.Info("AutoGladiators MAUI app initialized successfully");
                
                // Register Shell routes for navigation
                Routing.RegisterRoute("ExplorationPage", typeof(ExplorationPage));
                Routing.RegisterRoute("AdventurePage", typeof(AdventurePage)); // Keep old route for compatibility
                Routing.RegisterRoute("BotRosterPage", typeof(BotRosterPage));
                Routing.RegisterRoute("InventoryPage", typeof(InventoryPage));
                
                logger.Info("Shell routes registered successfully");
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
