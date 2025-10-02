using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoGladiators.Client.ViewModels
{
    public class VictoryViewModel : INotifyPropertyChanged
    {
        private string _enemyDefeatedText = "";
        private string _xpRewardText = "";
        private string _goldRewardText = "";

        public string EnemyDefeatedText
        {
            get => _enemyDefeatedText;
            set { _enemyDefeatedText = value; OnPropertyChanged(); }
        }

        public string XpRewardText
        {
            get => _xpRewardText;
            set { _xpRewardText = value; OnPropertyChanged(); }
        }

        public string GoldRewardText
        {
            get => _goldRewardText;
            set { _goldRewardText = value; OnPropertyChanged(); }
        }

        public ICommand ContinueCommand { get; }

        public VictoryViewModel(string enemyName, int enemyLevel, int xpGained, int goldGained)
        {
            EnemyDefeatedText = $"You defeated {enemyName} (Level {enemyLevel})!";
            XpRewardText = $"+{xpGained} Experience Points";
            GoldRewardText = $"+{goldGained} Gold";

            ContinueCommand = new Command(async () => await OnContinue());
        }

        private async Task OnContinue()
        {
            // Return to the previous page (Adventure)
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}