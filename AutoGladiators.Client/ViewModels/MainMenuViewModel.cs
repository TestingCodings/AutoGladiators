using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AutoGladiators.Client.ViewModels
{
    public partial class MainMenuViewModel : ObservableObject
    {
        public ICommand StartAdventureCommand { get; }
        public ICommand ViewBotsCommand { get; }
        public ICommand OpenInventoryCommand { get; }

        public MainMenuViewModel()
        {
            StartAdventureCommand = new RelayCommand(OnStartAdventure);
            ViewBotsCommand = new RelayCommand(OnViewBots);
            OpenInventoryCommand = new RelayCommand(OnOpenInventory);
        }

        private void OnStartAdventure()
        {
            Shell.Current.GoToAsync("AdventurePage");
        }

        private void OnViewBots()
        {
            Shell.Current.GoToAsync("BotRosterPage");
        }

        private void OnOpenInventory()
        {
            Shell.Current.GoToAsync("InventoryPage");
        }
    }
}
// This ViewModel handles the main menu actions and navigation in the application.
// It uses CommunityToolkit.Mvvm to define commands for starting an adventure, viewing bots, and opening the inventory.
// Each command navigates to the corresponding page using Shell.Current.GoToAsync, allowing for a clean and organized navigation structure.
// The MainMenuViewModel is designed to be used with a MainMenuPage, where these commands can be bound to buttons or other UI elements.
