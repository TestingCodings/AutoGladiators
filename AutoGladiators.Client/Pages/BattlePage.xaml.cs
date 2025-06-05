using Microsoft.Maui.Controls;
using System;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        int enemyHealth = 100;
        string enemyName = "OmegaX";

        public BattlePage()
        {
            InitializeComponent();
        }

        private void LogAction(string message)
        {
            BattleLog.Text += $"{message}\n";
        }

        private void OnPowerStrike(object sender, EventArgs e)
        {
            int damage = new Random().Next(10, 20);
            enemyHealth -= damage;
            EnemyHealth.Text = $"HP: {enemyHealth}";
            LogAction($"You used Power Strike! {enemyName} took {damage} damage.");
        }

        private void OnEvade(object sender, EventArgs e)
        {
            LogAction("You attempt to evade the next attack!");
        }

        private void OnRepair(object sender, EventArgs e)
        {
            LogAction("Your bot repairs itself slightly.");
        }

        private void OnCapture(object sender, EventArgs e)
        {
            LogAction("You threw a Control Chip... It's shaking...");
            bool success = new Random().NextDouble() > 0.6;

            if (success)
                LogAction($"Capture successful! {enemyName} has been added to your roster.");
            else
                LogAction($"Capture failed. {enemyName} broke free.");
        }
    }
}
