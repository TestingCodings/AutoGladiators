using Microsoft.Maui.Controls;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using System;
using System.Threading.Tasks;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        private BattleViewModel? _viewModel;
        
        // Default ctor: fetch bots from GameStateService
        public BattlePage()
        {
            InitializeComponent();

            var gs = GameStateService.Instance;
            var player = gs.GetCurrentBot() ?? new GladiatorBot { Id = 1, Name = "Champion", Level = 5, CurrentHealth = 100, MaxHealth = 100 };
            var enemy  = gs.GetEncounteredBot() ?? new GladiatorBot { Id = 2, Name = "Wild Scrapper", Level = 3, CurrentHealth = 75, MaxHealth = 75 };

            SetupBattle(player, enemy);
        }

        // Overload if callers want to pass bots directly
        public BattlePage(GladiatorBot player, GladiatorBot enemy)
        {
            InitializeComponent();
            SetupBattle(player, enemy);
        }
        
        private void SetupBattle(GladiatorBot player, GladiatorBot enemy)
        {
            _viewModel = new BattleViewModel(player, enemy);
            BindingContext = _viewModel;
            
            // Subscribe to battle events for animations
            _viewModel.DamageDealt += OnDamageDealt;
            _viewModel.BattleEnded += OnBattleEnded;
        }
        
        private async void OnDamageDealt(object? sender, string damageText)
        {
            // Show damage indicator animation
            if (DamageIndicator != null)
            {
                DamageIndicator.Text = damageText;
                DamageIndicator.TextColor = damageText.StartsWith("-") ? Colors.Red : Colors.LimeGreen;
                DamageIndicator.IsVisible = true;
                
                // Animate the damage indicator
                await DamageIndicator.ScaleTo(1.3, 250);
                await DamageIndicator.ScaleTo(1.0, 250);
                await Task.Delay(1000);
                
                DamageIndicator.IsVisible = false;
            }
        }
        
        private async void OnBattleEnded(object? sender, EventArgs e)
        {
            // Trigger battle end animations
            if (BattleEndOverlay != null)
            {
                BattleEndOverlay.Opacity = 0;
                BattleEndOverlay.IsVisible = true;
                await BattleEndOverlay.FadeTo(1.0, 500);
            }
        }
        
        protected override void OnDisappearing()
        {
            // Cleanup event subscriptions
            if (_viewModel != null)
            {
                _viewModel.DamageDealt -= OnDamageDealt;
                _viewModel.BattleEnded -= OnBattleEnded;
            }
            base.OnDisappearing();
        }
    }
}
