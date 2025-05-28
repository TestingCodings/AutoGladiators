using Microsoft.Maui.Controls;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.Core.Behaviors;
using System;
using System.Collections.Generic;

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

            // Display the result in a Label
            ResultLabel.Text = $"🏆 Winner: {result.Winner}\nOutcome: {result.Outcome}\n\n{string.Join("\n", result.Log)}";
        }
    }
}
