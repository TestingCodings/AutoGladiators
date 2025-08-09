using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.Logic
{
    public class BattleManager
    {
        public GladiatorBot PlayerBot { get; }
        public GladiatorBot EnemyBot { get; }
        public bool IsWaitingForPlayerInput { get; private set; }
        public bool BattleIsOver => PlayerBot.IsFainted || EnemyBot.IsFainted;
        public bool PlayerWon => EnemyBot.IsFainted && !PlayerBot.IsFainted;
        public bool IsBattleOver => PlayerBot.IsFainted || EnemyBot.IsFainted;

        public BattleManager(GladiatorBot player, GladiatorBot enemy)
        {
            PlayerBot = player;
            EnemyBot = enemy;
            Initialize();
        }

        public void Initialize()
        {
            // Reset bots, HUD, etc.
            PlayerBot.IsFainted = false;
            EnemyBot.IsFainted = false;
            // ...other setup...
        }

        public async Task BeginTurnAsync()
        {
            IsWaitingForPlayerInput = true;
            // Wait for player input (handled by UI)
        }

        public async Task ResolveTurnAsync(Move playerMove, Move enemyMove)
        {
            IsWaitingForPlayerInput = false;
            // Player attacks
            playerMove.Use(PlayerBot, EnemyBot);
            if (BattleIsOver) return;
            // Enemy attacks
            enemyMove.Use(EnemyBot, PlayerBot);
        }

        public BattleRewards GenerateBattleRewards()
        {
            // Example: XP and gold based on enemy
            int xp = EnemyBot.Level * 10;
            int gold = EnemyBot.Level * 5;
            return new BattleRewards { Experience = xp, Gold = gold };
        }

        public void SwitchToPlayerMode() { /* Optional: implement if needed */ }

        public Move? ChooseMove(GladiatorBot bot)
        {
            // Simple AI: pick the first available move
            var moves = bot.Moveset;
            if (moves != null && moves.Count > 0)
            {
                // If Moveset is List<string>, adapt as needed to return a Move object
                // Here we assume you have a MoveDatabase or similar to resolve names to Move
                var moveName = moves[0];
                return MoveDatabase.GetMoveByName(moveName);
            }
            return null;
        }
    }

    public class BattleRewards
    {
        public int Experience { get; set; }
        public int Gold { get; set; }
        // Add loot/items if needed
    }

    // ------------------ AI Battle Logic (separate from BattleManager) ------------------

    public class BattleAI
    {
        private readonly Random rng = new Random();
        private GladiatorBot player;
        private GladiatorBot enemy;

        public void Initialize(GladiatorBot player, GladiatorBot enemy)
        {
            this.player = player;
            this.enemy = enemy;
        }

        public void PerformTurn()
        {
            if (enemy.IsFainted) return;

            // Simple AI: randomly choose one of the enemy's moves
            var move = ChooseMove(enemy);
            move?.Use(enemy, player);
        }

        private Move? ChooseMove(GladiatorBot bot)
        {
            // Simple AI: pick the first available move
            var moves = bot.Moveset;
            if (moves != null && moves.Count > 0)
            {
                // If Moveset is List<string>, adapt as needed to return a Move object
                // Here we assume you have a MoveDatabase or similar to resolve names to Move
                var moveName = moves[0];
                return MoveDatabase.GetMoveByName(moveName);
            }
            return null;
        }

        // ---------------- helpers: flexible model access ----------------

        private IEnumerable<Move> GetMoves(GladiatorBot bot)
        {
            // LearnableMoves as IEnumerable<Move>
            var learnableProp = bot.GetType().GetProperty("LearnableMoves");
            if (learnableProp?.GetValue(bot) is IEnumerable<Move> lm) return lm;

            // LearnableMoves as IEnumerable<string>
            if (learnableProp?.GetValue(bot) is IEnumerable<string> lms)
                return lms.Select(n => new Move { Name = n });

            // Moves as IEnumerable<Move>
            var movesProp = bot.GetType().GetProperty("Moves");
            if (movesProp?.GetValue(bot) is IEnumerable<Move> mv) return mv;

            // Moves as IEnumerable<string>
            if (movesProp?.GetValue(bot) is IEnumerable<string> mvs)
                return mvs.Select(n => new Move { Name = n });

            // MoveNames
            var namesProp = bot.GetType().GetProperty("MoveNames");
            if (namesProp?.GetValue(bot) is IEnumerable<string> names)
                return names.Select(n => new Move { Name = n });

            return Enumerable.Empty<Move>();
        }
    }

    // NOTE: Ensure your enum contains at least these; if it doesn’t, add 'Neutral' to your enum.
    public enum ElementalType { Neutral, Fire, Water, Grass, Nature }

    // --- Minimal MoveDatabase for move lookup ---
    public static class MoveDatabase
    {
        // Replace this with your actual move lookup logic as needed
        public static Move? GetMoveByName(string name)
        {
            // Example: return a dummy Move with just the name set
            return new Move { Name = name };
        }
    }
}
