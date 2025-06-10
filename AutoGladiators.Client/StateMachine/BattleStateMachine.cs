using System;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    public class BattleStateMachine
    {
        // Add a constructor that takes 3 arguments (adjust types/names as needed)
        public BattleStateMachine(GladiatorBot playerBot, GladiatorBot enemyBot, Random rng = null)
        {
            PlayerBot = playerBot;
            EnemyBot = enemyBot;
            if (rng != null)
                _rng = rng;
        }
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

            int damage = int.Parse(PlayerBot.Attack(EnemyBot));
            EnemyBot.CurrentHealth -= damage;

            Log($"You attacked {EnemyBot.Name} for {damage} damage.");

            if (EnemyBot.CurrentHealth <= 0)
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

            int damage = int.Parse(EnemyBot.Attack(PlayerBot));
            PlayerBot.CurrentHealth -= damage;

            Log($"{EnemyBot.Name} attacked you for {damage} damage.");

            if (PlayerBot.CurrentHealth <= 0)
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
// This code defines a simple state machine for handling battles in the Auto Gladiators game.
// It manages the battle states, player and enemy actions, and captures attempts.
// The BattleStateMachine class allows for starting battles, handling player and enemy turns, and capturing enemy bots.
// It also provides events for logging battle messages, changing states, and ending battles.
// The battle flow includes player attacks, enemy responses, and capture attempts, with state transitions based on the outcomes of actions.
// The BattleStateMachine can be integrated into a game UI to provide real-time feedback and control over the battle process.
// The code is designed to be extensible, allowing for additional features like special attacks, power-ups, or more complex battle mechanics in the future.
// The BattleStateMachine can be used in a mobile game to create engaging and interactive battles between player-controlled bots and enemy bots.
// It can be further enhanced with animations, sound effects, and visual feedback to improve the player experience.
// The BattleStateMachine is a core component of the Auto Gladiators game, enabling dynamic and engaging battles.
// It can be tested and modified to suit different gameplay mechanics or to introduce new features as the game evolves.
// The code is structured to allow easy integration with other components of the game, such as player inventory, bot management, and game state transitions.
// The BattleStateMachine can be used in various game modes, such as single-player campaigns, multiplayer battles, or training sessions.
// It provides a solid foundation for building a rich and interactive battle system in the Auto Gladiators game.
// The BattleStateMachine can be extended to include more complex battle mechanics, such as elemental advantages, special abilities, or status effects.
// It can also be adapted for different game genres, such as RPGs, strategy games, or action games, by modifying the battle logic and state transitions.
// The code is designed to be modular and maintainable, allowing for easy updates and enhancements as the game develops.