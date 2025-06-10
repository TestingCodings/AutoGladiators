using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private async void OnAdventureClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdventurePage());
        }

        private async void OnBotRosterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BotRosterPage());
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InventoryPage());
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Coming Soon", "Settings will be available in a future update.", "OK");
        }

        //private async void OnSimulatorClicked(object sender, EventArgs e)
        //{
           // await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
        //}
    }
}
