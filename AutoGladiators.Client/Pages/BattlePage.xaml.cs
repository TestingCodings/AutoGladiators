using Microsoft.Maui.Controls;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        // Default ctor: fetch bots from GameStateService
        public BattlePage()
        {
            InitializeComponent();

            var gs = GameStateService.Instance;
            var player = gs.GetCurrentBot() ?? new GladiatorBot { Id = 1, Name = "Player Bot", Level = 1, CurrentHealth = 10, MaxHealth = 10 };
            var enemy  = gs.GetEncounteredBot() ?? new GladiatorBot { Id = 2, Name = "Enemy Bot",  Level = 1, CurrentHealth = 10, MaxHealth = 10 };

            BindingContext = new BattleViewModel(player, enemy);
        }

        // Overload if callers want to pass bots directly
        public BattlePage(GladiatorBot player, GladiatorBot enemy) : this()
        {
            BindingContext = new BattleViewModel(player, enemy);
        }
    }
}
