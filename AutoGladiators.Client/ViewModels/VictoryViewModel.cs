using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using AutoGladiators.Core.Services;

namespace AutoGladiators.Client.ViewModels
{
    public class VictoryViewModel : INotifyPropertyChanged
    {
        private string _enemyDefeatedText = "";
        private string _xpRewardText = "";
        private string _goldRewardText = "";
        private string _levelUpText = "";
        private string _statGrowthText = "";
        private bool _hasLeveledUp = false;
        private bool _hasNewItems = false;

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

        public string LevelUpText
        {
            get => _levelUpText;
            set { _levelUpText = value; OnPropertyChanged(); }
        }

        public string StatGrowthText
        {
            get => _statGrowthText;
            set { _statGrowthText = value; OnPropertyChanged(); }
        }

        public bool HasLeveledUp
        {
            get => _hasLeveledUp;
            set { _hasLeveledUp = value; OnPropertyChanged(); }
        }

        public bool HasNewItems
        {
            get => _hasNewItems;
            set { _hasNewItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> NewItems { get; } = new();

        public ICommand ContinueCommand { get; }

        public VictoryViewModel(string enemyName, int enemyLevel, int xpGained, int goldGained, 
                              LevelUpResult? levelUpResult = null, List<string>? newItems = null)
        {
            EnemyDefeatedText = $"You defeated {enemyName} (Level {enemyLevel})!";
            XpRewardText = $"+{xpGained} Experience Points";
            GoldRewardText = $"+{goldGained} Gold";

            if (levelUpResult != null && levelUpResult.HasLeveledUp)
            {
                HasLeveledUp = true;
                LevelUpText = levelUpResult.LevelsGained == 1 
                    ? $"🎉 LEVEL UP! 🎉\nReached Level {levelUpResult.NewLevel}!"
                    : $"🎉 MULTIPLE LEVEL UPS! 🎉\nGained {levelUpResult.LevelsGained} levels! Now Level {levelUpResult.NewLevel}!";
                
                StatGrowthText = $"Stat Growth: {levelUpResult.StatGrowth}";
            }

            if (newItems != null && newItems.Any())
            {
                HasNewItems = true;
                foreach (var item in newItems)
                {
                    NewItems.Add(item);
                }
            }

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