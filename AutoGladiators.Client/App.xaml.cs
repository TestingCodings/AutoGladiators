using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
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
                        // Use proper logging instead of Console.WriteLine - fallback to Debug if AppLog isn't ready
                        try
                        {
                            var logger = AppLog.For<App>();
                            logger.Error($"Error initializing game loop: {ex.Message}", ex);
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"Error initializing game loop: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    }
                });

                // Create a temporary main page that will resolve services later
                MainPage = new ContentPage 
                { 
                    Content = new ActivityIndicator 
                    { 
                        IsRunning = true, 
                        VerticalOptions = LayoutOptions.Center 
                    } 
                };

            }
            catch (Exception ex)
            {
                // Log startup errors
                System.Diagnostics.Debug.WriteLine($"App initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to prevent app from starting in bad state
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            // Now the handler and services should be properly initialized
            Task.Run(async () =>
            {
                try
                {
                    // Small delay to ensure everything is fully initialized
                    await Task.Delay(100);
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            // Get the PlayerProfileService from the DI container
                            var serviceProvider = Handler?.MauiContext?.Services;
                            if (serviceProvider != null)
                            {
                                var profileService = serviceProvider.GetService<PlayerProfileService>();
                                if (profileService != null)
                                {
                                    MainPage = new NavigationPage(new MainMenuPage(profileService));
                                }
                                else
                                {
                                    // Fallback - try to create instance manually
                                    MainPage = new NavigationPage(new MainMenuPage(PlayerProfileService.Instance));
                                }
                            }
                            else
                            {
                                // Fallback - try to create instance manually
                                MainPage = new NavigationPage(new MainMenuPage(PlayerProfileService.Instance));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to resolve services: {ex.Message}");
                            // Last resort fallback
                            MainPage = new NavigationPage(new MainMenuPage(PlayerProfileService.Instance));
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnStart service resolution failed: {ex.Message}");
                }
            });
        }
    }
}
// This code initializes the application and sets the main page to a navigation stack containing the MainMenuPage.
// The MainMenuPage serves as the entry point for the user to navigate to different parts of the application, such as the adventure page, bot roster, inventory, and settings.
