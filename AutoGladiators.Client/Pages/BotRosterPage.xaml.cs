using Microsoft.Maui.Controls;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.Core.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoGladiators.Client.Pages
{
    public partial class SimulationPage : ContentPage
    {
        public SimulationPage()
        {
            InitializeComponent();
        }

        private void OnRunSimulationClicked(object sender, EventArgs e)
        {
            var bot1 = new GladiatorBot("Bot Alpha", new AggressiveBehavior(), 100, 50);
            var bot2 = new GladiatorBot("Bot Omega", new DefensiveBehavior(), 100, 50);
            var result = BattleSimulator.SimulateBattle(bot1, bot2);

            WinnerLabel.Text = $"🏆 Winner: {result.Winner}";
            OutcomeLabel.Text = $"Outcome: {result.Outcome}";

            BattleLogLabel.Text = string.Join("\n", result.Log);

            var statsText = string.Join("\n\n", result.BotStats.Select(kvp =>
                $"- {kvp.Key} -" +
                string.Join("\n", kvp.Value.Select(stat => $"{stat.Key}: {stat.Value}"))
            ));

            StatsSummaryLabel.Text = statsText;
        }
    }
}
