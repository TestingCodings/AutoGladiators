using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;


namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class BattlingState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<BattlingState>();

        public GameStateId Id => GameStateId.Battling;

        private readonly GameStateService _game = GameStateService.Instance;

        private GladiatorBot? _player;
        private GladiatorBot? _enemy;
        private bool _resolved;
        private StateTransition? _pending;

        public async Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _resolved = false;
            _pending = null;

            _player = _game.GetCurrentBot();
            _enemy = _game.CurrentEncounter;

            if (_player is null || _enemy is null)
            {
                Log.LogWarning("Battle could not start: missing player or enemy bot.");
                _pending = new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "NoBattleBots" }
                );
                _resolved = true;
                return;
            }

            ctx.Ui?.ShowBattleHud(_player, _enemy);
            ctx.Ui?.SetStatus($"Battle started: {_player.Name} vs {_enemy.Name}");

            // Run one-turn battle
            var battleManager = new AutoGladiators.Core.Logic.BattleManager(_player, _enemy);
            var battleLog = await battleManager.RunOneTurnBattleAsync();
            Log.LogInformation(battleLog);

            bool playerWon = _player.CurrentHealth > 0 && _enemy.CurrentHealth <= 0;
            bool playerLost = _enemy.CurrentHealth > 0 && _player.CurrentHealth <= 0;

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
                
                _game.ApplyBattleRewards(totalXp, totalGold);
                _game.SetLastBattleStats(_player, _enemy, true);
                _game.SetFlag("LastBattleOutcome", true);
                
                Log.LogInformation($"Victory rewards calculated: {totalXp} XP (base: {baseXp}, bonus: {xpBonus}), {totalGold} Gold (base: {baseGold})");
                
                _pending = new StateTransition(
                    GameStateId.Victory,
                    new StateArgs { 
                        Reason = "BattleWon", 
                        Payload = new { 
                            xp = totalXp, 
                            gold = totalGold, 
                            enemyId = _enemy.Id,
                            enemyName = _enemy.Name,
                            enemyLevel = _enemy.Level 
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
                    new StateArgs { Reason = "BattleLost", Payload = new { enemyId = _enemy.Id } }
                );
            }
            else
            {
                // Draw or both standing: treat as defeat for now
                _game.SetLastBattleStats(_player, _enemy, false);
                _game.SetFlag("LastBattleOutcome", false);
                _pending = new StateTransition(
                    GameStateId.Defeat,
                    new StateArgs { Reason = "BattleDraw", Payload = new { enemyId = _enemy.Id } }
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

