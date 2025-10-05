using Microsoft.Maui.Controls;
using AutoGladiators.Client.Pages;
using AutoGladiators.Client.Services;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Client
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                
                // Don't use DependencyService with DI - services are already registered in MauiProgram
                // DependencyService.Register<GameStateService>();

                // Initialize the UI bridge for the state machine
                var uiBridge = new MauiUiBridge();
                
                // Initialize game loop in background but don't block app startup
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await GameLoop.InitializeAsync(uiBridge);
                    }
                    catch (Exception ex)
                    {
                        // Use proper logging instead of Console.WriteLine
                        var logger = AppLog.For<App>();
                        logger.Error($"Error initializing game loop: {ex.Message}", ex);
                        logger.Error($"Stack trace: {ex.StackTrace}");
                    }
                });

                // Set the root of the app to the Main Menu wrapped in a navigation stack
                // Get the PlayerProfileService from the DI container
                var profileService = Handler?.MauiContext?.Services?.GetService<PlayerProfileService>();
                if (profileService != null)
                {
                    MainPage = new NavigationPage(new MainMenuPage(profileService));
                }
                else
                {
                    throw new InvalidOperationException("PlayerProfileService not found in DI container");
                }
            }
            catch (Exception ex)
            {
                // Log startup errors
                System.Diagnostics.Debug.WriteLine($"App initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to prevent app from starting in bad state
            }
        }
    }
}
// This code initializes the application and sets the main page to a navigation stack containing the MainMenuPage.
// The MainMenuPage serves as the entry point for the user to navigate to different parts of the application, such as the adventure page, bot roster, inventory, and settings.
