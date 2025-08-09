using Microsoft.Maui.Controls;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Client.Converters;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        public BattlePage(GladiatorBot playerBot, GladiatorBot enemyBot)
        {
            InitializeComponent();
            BindingContext = new BattleViewModel(playerBot, enemyBot);
        }
    }
}
