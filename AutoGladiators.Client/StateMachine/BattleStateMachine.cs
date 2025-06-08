using System;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    public class BattleStateMachine
    {
        public enum BattleState
        {
            Start,
            PlayerTurn,
            EnemyTurn,
            Win,
            Lose,
            CaptureAttempt
        }

        public BattleState CurrentState { get; private set; } = BattleState.Start;

        public GladiatorBot PlayerBot { get; private set; }
        public GladiatorBot EnemyBot { get; private set; }

        public event Action<string> OnBattleLogUpdated;
        public event Action<BattleState> OnStateChanged;
        public event Action OnBattleEnded;

        private readonly Random _rng = new();

        public void StartBattle(GladiatorBot playerBot, GladiatorBot enemyBot)
        {
            PlayerBot = playerBot;
            EnemyBot = enemyBot;

            Log($"A wild {EnemyBot.Name} appeared!");
            ChangeState(BattleState.PlayerTurn);
        }

        public void PlayerAttack()
        {
            if (CurrentState != BattleState.PlayerTurn) return;

            int damage = PlayerBot.Attack;
            EnemyBot.Health -= damage;

            Log($"You attacked {EnemyBot.Name} for {damage} damage.");

            if (EnemyBot.Health <= 0)
            {
                Log("You won the battle!");
                ChangeState(BattleState.Win);
                EndBattle();
            }
            else
            {
                ChangeState(BattleState.EnemyTurn);
                EnemyAttack();
            }
        }

        public void EnemyAttack()
        {
            if (CurrentState != BattleState.EnemyTurn) return;

            int damage = EnemyBot.Attack;
            PlayerBot.Health -= damage;

            Log($"{EnemyBot.Name} attacked you for {damage} damage.");

            if (PlayerBot.Health <= 0)
            {
                Log("You lost the battle...");
                ChangeState(BattleState.Lose);
                EndBattle();
            }
            else
            {
                ChangeState(BattleState.PlayerTurn);
            }
        }

        public void AttemptCapture()
        {
            if (CurrentState != BattleState.PlayerTurn) return;

            ChangeState(BattleState.CaptureAttempt);

            bool success = _rng.NextDouble() < 0.5;

            if (success)
            {
                Log($"You captured {EnemyBot.Name}!");
                ChangeState(BattleState.Win);
                EndBattle();
            }
            else
            {
                Log("Capture attempt failed!");
                ChangeState(BattleState.EnemyTurn);
                EnemyAttack();
            }
        }

        private void ChangeState(BattleState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void Log(string message)
        {
            OnBattleLogUpdated?.Invoke(message);
        }

        private void EndBattle()
        {
            OnBattleEnded?.Invoke();
        }
    }
}
