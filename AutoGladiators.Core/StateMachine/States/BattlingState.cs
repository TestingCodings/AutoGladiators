using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Analytics;
using Microsoft.Extensions.Logging;


namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class BattlingState : IGameState
    {
        private static readonly IAppLogger _logger = AppLog.For<BattlingState>();
        private static readonly IBattleAnalytics _analytics = new BattleAnalytics();
        private static readonly IPerformanceMonitor _performance = new PerformanceMonitor();

        public GameStateId Id => GameStateId.Battling;

        private readonly GameStateService _game = GameStateService.Instance;

        private GladiatorBot? _player;
        private GladiatorBot? _enemy;
        private bool _resolved;
        private StateTransition? _pending;
        private DateTime _battleStartTime;

        public async Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            using var _ = _performance.StartOperation("BattleState.Enter");
            
            _resolved = false;
            _pending = null;
            _battleStartTime = DateTime.UtcNow;

            _player = _game.GetCurrentBot();
            _enemy = _game.CurrentEncounter;

            if (_player is null || _enemy is null)
            {
                var errorData = new Dictionary<string, object?>
                {
                    ["PlayerPresent"] = _player != null,
                    ["EnemyPresent"] = _enemy != null
                };
                
                _logger.LogError("BattleState", "MissingBattleBots", null, errorData);
                
                _pending = new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "NoBattleBots" }
                );
                _resolved = true;
                return;
            }

            // Log battle analytics
            _analytics.LogBattleStart(_player, _enemy);

            ctx.Ui?.ShowBattleHud(_player, _enemy);
            ctx.Ui?.SetStatus($"Battle started: {_player.Name} vs {_enemy.Name}");

            // Run one-turn battle
            var perfData = new Dictionary<string, object?>
            {
                ["PlayerLevel"] = _player?.Level ?? 0,
                ["EnemyLevel"] = _enemy?.Level ?? 0
            };
            
            using var battlePerf = _performance.StartOperation("BattleExecution", perfData);
            
            var battleManager = new AutoGladiators.Core.Logic.BattleManager(_player, _enemy);
            var battleLog = await battleManager.RunOneTurnBattleAsync();
            
            _logger.Info($"Battle completed: {battleLog}");

            bool playerWon = _player.CurrentHealth > 0 && _enemy.CurrentHealth <= 0;
            bool playerLost = _enemy.CurrentHealth > 0 && _player.CurrentHealth <= 0;

            // Log battle completion analytics
            var battleDuration = DateTime.UtcNow - _battleStartTime;
            _analytics.LogBattleEnd(_player, _enemy, playerWon, battleDuration);

            if (playerWon)
            {
                // Enhanced reward calculation
                int baseXp = Math.Max(5, _enemy.Level * 10);
                int levelDifference = _enemy.Level - _player.Level;
                
                // Bonus XP for defeating higher level enemies
                int xpBonus = Math.Max(0, levelDifference * 5);
                int totalXp = baseXp + xpBonus;
                
                // Gold calculation with some randomness
                var random = new Random();
                int baseGold = Math.Max(2, _enemy.Level * 5);
                int goldVariance = random.Next(-2, 3); // -2 to +2 variance
                int totalGold = Math.Max(1, baseGold + goldVariance);
                
                // Apply progression system
                _player.Experience += totalXp;
                var progressionService = new BotProgressionService();
                var levelUpResult = progressionService.TryLevelUp(_player);

                // Add random items to inventory after victory
                var inventoryService = InventoryService.Instance;
                if (random.NextDouble() < 0.3) // 30% chance for healing potion
                {
                    inventoryService.AddItem(new HealingPotion(50 + random.Next(0, 26))); // 50-75 heal
                }
                if (random.NextDouble() < 0.15) // 15% chance for energy potion
                {
                    inventoryService.AddItem(new EnergyPotion(30 + random.Next(0, 21))); // 30-50 energy
                }
                
                _game.ApplyBattleRewards(totalXp, totalGold);
                _game.SetLastBattleStats(_player, _enemy, true);
                _game.SetFlag("LastBattleOutcome", true);
                
                // Log rewards analytics including level up
                _analytics.LogRewardsEarned(_player.Id.ToString(), totalXp, totalGold, _enemy.Level);
                if (levelUpResult.HasLeveledUp)
                {
                    _analytics.LogPlayerLevelUp(_player.Id.ToString(), levelUpResult.NewLevel, _player.Experience);
                }
                
                var victoryData = new Dictionary<string, object?>
                {
                    ["XpEarned"] = totalXp,
                    ["BaseXp"] = baseXp,
                    ["XpBonus"] = xpBonus,
                    ["GoldEarned"] = totalGold,
                    ["BaseGold"] = baseGold,
                    ["LevelsGained"] = levelUpResult?.LevelsGained ?? 0,
                    ["NewLevel"] = levelUpResult?.NewLevel ?? _player.Level,
                    ["StatGrowth"] = levelUpResult?.StatGrowth?.ToString() ?? "None",
                    ["BattleDurationSeconds"] = battleDuration.TotalSeconds
                };
                
                _logger.LogBattleEvent("Victory", _player.Id.ToString(), _enemy.Id.ToString(), victoryData);
                
                _pending = new StateTransition(
                    GameStateId.Victory,
                    new StateArgs { 
                        Reason = "BattleWon", 
                        Payload = new { 
                            xp = totalXp, 
                            gold = totalGold, 
                            enemyId = _enemy?.Id ?? 0,
                            enemyName = _enemy?.Name ?? "Unknown",
                            enemyLevel = _enemy?.Level ?? 1 
                        } 
                    }
                );
            }
            else if (playerLost)
            {
                _game.SetLastBattleStats(_player, _enemy, false);
                _game.SetFlag("LastBattleOutcome", false);
                _pending = new StateTransition(
                    GameStateId.Defeat,
                    new StateArgs { Reason = "BattleLost", Payload = new { enemyId = _enemy?.Id ?? 0 } }
                );
            }
            else
            {
                // Draw or both standing: treat as defeat for now
                _game.SetLastBattleStats(_player, _enemy, false);
                _game.SetFlag("LastBattleOutcome", false);
                _pending = new StateTransition(
                    GameStateId.Defeat,
                    new StateArgs { Reason = "BattleDraw", Payload = new { enemyId = _enemy?.Id ?? 0 } }
                );
            }

            _resolved = true;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Immediately hand off to the next state once we've prepared the result.
            if (_resolved)
            {
                var next = _pending;
                _pending = null;
                _resolved = false;
                return Task.FromResult(next);
            }

            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideBattleHud();
            return Task.CompletedTask;
        }
    }
}

