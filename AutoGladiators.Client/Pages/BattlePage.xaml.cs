using Microsoft.Maui.Controls;
using System;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class BattlePage : ContentPage
    {
        private GladiatorBot _enemyBot;
        private BattleStateMachine _battleStateMachine;


        
        private void PlayIntroAnimation()
        {
            // Stub: Replace with your actual animation logic
            LogAction($"A wild {_enemyBot.Name} appears!");
        }

        private void StartBattle()
        {
            _battleStateMachine = new BattleStateMachine(_enemyBot, OnBattleEvent);
            _battleStateMachine.StartBattle();
        }

        private void OnBattleEvent(string message)
        {
            LogAction(message);
            EnemyHealth.Text = $"HP: {_enemyBot.MaxHealth}";
        }
        int enemyHealth = 100;
        string enemyName = "OmegaX";

        public BattlePage(GladiatorBot encounteredBot)
        {
            InitializeComponent();
            _enemyBot = encounteredBot;
            EnemyHealth.Text = $"HP: {_enemyBot.MaxHealth}";
            enemyName = _enemyBot.Name;

            PlayIntroAnimation();
            StartBattle();
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
