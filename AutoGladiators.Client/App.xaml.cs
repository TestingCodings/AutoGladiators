using Microsoft.Maui.Controls;
using AutoGladiators.Client.Pages;
using AutoGladiators.Core.Services;

namespace AutoGladiators.Client
{
    public partial class App : Application
    {
        public App()
        {

            InitializeComponent(); // must match the x:Class in App.xaml
            DependencyService.Register<GameStateService>();

            // Set the root of the app to the Main Menu wrapped in a navigation stack
            MainPage = new NavigationPage(new MainMenuPage());
        }
    }
}
// This code initializes the application and sets the main page to a navigation stack containing the MainMenuPage.
// The MainMenuPage serves as the entry point for the user to navigate to different parts of the application, such as the adventure page, bot roster, inventory, and settings.
