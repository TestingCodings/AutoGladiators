using Microsoft.Maui.Controls;
using System;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        private GladiatorBot _enemyBot;
        private GladiatorBot _playerBot;

        
        private BattleStateMachine _battleStateMachine;
        private string _enemyName;

        public BattlePage(GladiatorBot playerBot, GladiatorBot encounteredBot)
        {
            InitializeComponent();

            _playerBot = playerBot;
            _enemyBot = encounteredBot;

            PlayerNameLabel.Text = _playerBot.Name;
            EnemyNameLabel.Text = _enemyBot.Name;

            UpdateHealthDisplays();

            PlayIntroAnimation();
            StartBattle();
        }


        private void PlayIntroAnimation()
        {
            LogAction($"A wild {_enemyBot.Name} appears!");
        }

        private async void UpdateHealthDisplays()
        {
            PlayerHealth.Text = $"HP: {_playerBot.CurrentHealth}/{_playerBot.MaxHealth}";
            EnemyHealth.Text = $"HP: {_enemyBot.CurrentHealth}/{_enemyBot.MaxHealth}";

            double playerProgress = Math.Max(0, (double)_playerBot.CurrentHealth / _playerBot.MaxHealth);
            double enemyProgress = Math.Max(0, (double)_enemyBot.CurrentHealth / _enemyBot.MaxHealth);

            await PlayerHealthBar.ProgressTo(playerProgress, 250, Easing.CubicInOut);
            await EnemyHealthBar.ProgressTo(enemyProgress, 250, Easing.CubicInOut);
        }


        private void StartBattle()
        {
            _battleStateMachine = new BattleStateMachine(_playerBot, _enemyBot, OnBattleEvent);
            _battleStateMachine.StartBattle();
        }
        private void OnBattleEvent(string message)
        {
            LogAction(message);
            UpdateHealthDisplays();
            if (_enemyBot.CurrentHealth <= 0)
            {
                LogAction($"{_enemyBot.Name} has been defeated!");
                // Optionally, navigate to a victory page or end the battle
            }

        }

        private void OnPowerStrike(object sender, EventArgs e)
        {
            int damage = new Random().Next(10, 20);
            _enemyBot.ReceiveDamage(damage);
            UpdateHealthDisplays();

            LogAction($"You used Power Strike! {_enemyName} took {damage} damage.");
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
                LogAction($"Capture successful! {_enemyName} has been added to your roster.");
            else
                LogAction($"Capture failed. {_enemyName} broke free.");
        }



        private void LogAction(string message)
        {
            BattleLog.Text += $"{message}\n";
        }
    }
}
// This code defines a BattlePage in a mobile game where players can engage in battles with enemy bots.
// The page initializes with an enemy bot, displays its health, and allows players to perform actions like attacking, evading, repairing, or attempting to capture the bot.