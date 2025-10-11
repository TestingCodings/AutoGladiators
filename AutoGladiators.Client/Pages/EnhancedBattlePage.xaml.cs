using Microsoft.Maui.Controls;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Core.Core;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.Pages
{
    public partial class EnhancedBattlePage : ContentPage
    {
        public EnhancedBattleViewModel ViewModel => (EnhancedBattleViewModel)BindingContext;

        public EnhancedBattlePage()
        {
            InitializeComponent();
            Title = "Enhanced Battle";
            
            // Initialize FileLogger and ViewModel with it
            var fileLogger = new FileLogger();
            BindingContext = new EnhancedBattleViewModel(fileLogger);
        }

        public EnhancedBattlePage(GladiatorBot playerBot, GladiatorBot enemyBot) : this()
        {
            // Start the battle when page loads
            Loaded += (s, e) => ViewModel.StartBattle(playerBot, enemyBot);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Subscribe to battle events for animations/effects
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Unsubscribe from events
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle property changes for visual effects
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentBattleMessage):
                    await AnimateBattleMessage();
                    break;
                    
                case nameof(ViewModel.PlayerHealthPercentage):
                    await AnimateHealthChange(isPlayer: true);
                    break;
                    
                case nameof(ViewModel.EnemyHealthPercentage):
                    await AnimateHealthChange(isPlayer: false);
                    break;
            }
        }

        private async Task AnimateBattleMessage()
        {
            // Find the battle message frame and animate it
            try
            {
                var battleEffectsFrame = this.FindByName("BattleEffectsFrame") as Frame;
                if (battleEffectsFrame != null)
                {
                    // Pulse animation
                    await battleEffectsFrame.ScaleTo(1.05, 150, Easing.BounceOut);
                    await battleEffectsFrame.ScaleTo(1.0, 150, Easing.BounceOut);
                }
            }
            catch
            {
                // Animation failed, continue silently
            }
        }

        private async Task AnimateHealthChange(bool isPlayer)
        {
            try
            {
                // Find the appropriate health bar and animate damage/healing
                var targetFrame = isPlayer ? 
                    this.FindByName("PlayerArena") as Frame : 
                    this.FindByName("EnemyArena") as Frame;
                    
                if (targetFrame != null)
                {
                    // Flash red for damage, green for healing
                    var originalColor = targetFrame.BorderColor;
                    var flashColor = isPlayer ? Colors.Red : Colors.Orange;
                    
                    targetFrame.BorderColor = flashColor;
                    await targetFrame.FadeTo(0.7, 100);
                    await targetFrame.FadeTo(1.0, 100);
                    targetFrame.BorderColor = originalColor;
                }
            }
            catch
            {
                // Animation failed, continue silently
            }
        }

        // Helper method to create a test battle (for development)
        public static EnhancedBattlePage CreateTestBattle()
        {
            // Create test bots for demonstration
            var playerBot = new GladiatorBot
            {
                Name = "Thunder",
                Level = 5,
                MaxHealth = 150,
                CurrentHealth = 150,
                MaxMP = 65,
                CurrentMP = 65,
                MaxEnergy = 125,
                Energy = 125,
                AttackPower = 60,
                Defense = 35,
                Speed = 45
            };

            var enemyBot = new GladiatorBot
            {
                Name = "Shadow Stalker",
                Level = 4,
                MaxHealth = 120,
                CurrentHealth = 120,
                MaxMP = 50,
                CurrentMP = 50,
                MaxEnergy = 100,
                Energy = 100,
                AttackPower = 55,
                Defense = 30,
                Speed = 50
            };

            return new EnhancedBattlePage(playerBot, enemyBot);
        }
    }
}