using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Rng;


namespace AutoGladiators.Core.Logic
{
    public class BattleManager
    {
        private static readonly IAppLogger Log = AppLog.For<BattleManager>();

        public GladiatorBot Player { get; private set; }
        public GladiatorBot Enemy { get; private set; }
        private IRng _rng;

        public BattleManager(GladiatorBot player, GladiatorBot enemy, IRng? rng = null)
        {
            Player = player;
            Enemy = enemy;
            _rng = rng ?? new DefaultRng();
        }

        public bool IsPlayerWin() => Enemy.IsFainted && !Player.IsFainted;
        public bool IsPlayerLose() => Player.IsFainted && !Enemy.IsFainted;
        public bool IsTie() => Player.IsFainted && Enemy.IsFainted;
        public bool IsBattleOver() => Player.IsFainted || Enemy.IsFainted;

        public GladiatorBot[] DetermineTurnOrder()
        {
            // Default turn order based on speed
            return Player.Speed >= Enemy.Speed 
                ? new[] { Player, Enemy } 
                : new[] { Enemy, Player };
        }

        public (GladiatorBot User, Move Move)[] DetermineTurnOrder((GladiatorBot User, Move Move) playerChoice, (GladiatorBot User, Move Move) enemyChoice)
        {
            // Determine turn order based on priority, then speed
            var playerFirst = playerChoice.Move.Priority > enemyChoice.Move.Priority ||
                             (playerChoice.Move.Priority == enemyChoice.Move.Priority && playerChoice.User.Speed >= enemyChoice.User.Speed);

            return playerFirst 
                ? new[] { playerChoice, enemyChoice }
                : new[] { enemyChoice, playerChoice };
        }

        public void ResolveTurn(Move playerMove, Move enemyMove)
        {
            // Determine turn order
            var playerFirst = Player.Speed > Enemy.Speed || 
                             (Player.Speed == Enemy.Speed && playerMove.Priority > enemyMove.Priority);

            if (playerFirst)
            {
                ExecuteMove(Player, Enemy, playerMove);
                if (!Enemy.IsFainted)
                {
                    ExecuteMove(Enemy, Player, enemyMove);
                }
            }
            else
            {
                ExecuteMove(Enemy, Player, enemyMove);
                if (!Player.IsFainted)
                {
                    ExecuteMove(Player, Enemy, playerMove);
                }
            }
        }

        private void ExecuteMove(GladiatorBot attacker, GladiatorBot defender, Move move)
        {
            if (attacker.IsFainted || move == null) return;

            // Use the Move's built-in Use method which handles accuracy, damage, and feedback
            var result = move.Use(attacker, defender);
            
            // Log the battle action
            Log.Info(result);
        }

        public Move? ChooseMove(GladiatorBot bot)
        {
            // Simple AI: choose from available moves
            if (bot.Moveset != null && bot.Moveset.Count > 0)
            {
                // Pick a random move from the bot's moveset
                var moveIndex = _rng.Next(0, bot.Moveset.Count);
                var moveName = bot.Moveset[moveIndex];
                return MoveDatabase.GetMoveByName(moveName);
            }
            return null;
        }

        public async Task<string> RunOneTurnBattleAsync()
        {
            var battleLog = new System.Text.StringBuilder();
            
            battleLog.AppendLine($"Battle: {Player.Name} vs {Enemy.Name}");
            
            // Determine turn order
            var turnOrder = DetermineTurnOrder();
            
            foreach (var bot in turnOrder)
            {
                if (IsBattleOver()) break;
                
                if (bot == Player)
                {
                    // Player turn - choose a move
                    var playerMove = ChooseMove(Player);
                    if (playerMove != null)
                    {
                        ExecuteMove(Player, Enemy, playerMove);
                        battleLog.AppendLine($"{Player.Name} used {playerMove.Name}!");
                        
                        if (Enemy.IsFainted)
                        {
                            battleLog.AppendLine($"{Enemy.Name} has been defeated!");
                        }
                    }
                }
                else if (bot == Enemy)
                {
                    // Enemy turn - AI chooses move
                    var enemyMove = ChooseMove(Enemy);
                    if (enemyMove != null)
                    {
                        ExecuteMove(Enemy, Player, enemyMove);
                        battleLog.AppendLine($"{Enemy.Name} used {enemyMove.Name}!");
                        
                        if (Player.IsFainted)
                        {
                            battleLog.AppendLine($"{Player.Name} has been defeated!");
                        }
                    }
                }
                
                // Small delay for async behavior
                await Task.Delay(100);
            }
            
            if (IsBattleOver())
            {
                if (IsPlayerWin())
                    battleLog.AppendLine("Victory!");
                else if (IsPlayerLose())
                    battleLog.AppendLine("Defeat...");
                else
                    battleLog.AppendLine("Draw!");
            }
            
            return battleLog.ToString();
        }
    }

    public class BattleRewards
    {
        private static readonly IAppLogger Log = AppLog.For<BattleRewards>();

        public int Experience { get; set; }
        public int Gold { get; set; }
        // Add loot/items if needed
    }

    // ------------------ AI Battle Logic (separate from BattleManager) ------------------

    public class BattleAI
    {
        private static readonly IAppLogger Log = AppLog.For<BattleAI>();

        private readonly Random rng = new Random();
    private GladiatorBot? player;
    private GladiatorBot? enemy;

        public void Initialize(GladiatorBot player, GladiatorBot enemy)
        {
            this.player = player;
            this.enemy = enemy;
        }

        public void PerformTurn()
        {
            if (enemy == null || player == null) return;
            if (enemy.IsFainted) return;

            // Simple AI: randomly choose one of the enemy's moves
            var move = ChooseMove(enemy);
            if (move != null)
            {
                move.Use(enemy, player);
            }
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

    // --- MVP MoveDatabase for move lookup ---
    public static class MoveDatabase
    {
        private static readonly IAppLogger Log = AppLog.For("MoveDatabase");

        private static readonly Dictionary<string, Move> _moves;
        
        static MoveDatabase()
        {
            _moves = new Dictionary<string, Move>();
            
            // Load MVP moves
            var mvpMoves = Move.CreateMVPMoves();
            foreach (var move in mvpMoves)
            {
                _moves[move.Name] = move;
            }
        }
        
        public static Move? GetMoveByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            
            _moves.TryGetValue(name, out var move);
            return move;
        }
        
        public static List<Move> GetAllMoves()
        {
            return _moves.Values.ToList();
        }
    }
}

