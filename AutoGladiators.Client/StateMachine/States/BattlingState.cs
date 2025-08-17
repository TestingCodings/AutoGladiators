using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;


namespace AutoGladiators.Client.StateMachine.States
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

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _resolved = false;
            _pending = null;

            // Get bots from service (CurrentEncounter is already in your GameStateService)
            _player = _game.GetCurrentBot();
            _enemy = _game.CurrentEncounter;

            if (_player is null || _enemy is null)
            {
                // Nothing to battle; go back to exploring
                _pending = new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "NoBattleBots" }
                );
                return Task.CompletedTask;
            }

            // Show a simple HUD via your bridge
            ctx.Ui?.ShowBattleHud(_player, _enemy);
            ctx.Ui?.SetStatus($"Battle started: {_player.Name} vs {_enemy.Name}");

            // For now, decide an outcome immediately (stub logic)
            var playerPower = (_player.Level * 10) + _player.AttackPower;
            var enemyPower = (_enemy.Level * 10) + _enemy.AttackPower;

            var playerWon = playerPower >= enemyPower;

            if (playerWon)
            {
                // Simple rewards based on enemy level
                int xp = Math.Max(5, _enemy.Level * 10);
                int gold = Math.Max(2, _enemy.Level * 5);

                _game.ApplyBattleRewards(xp, gold);
                _game.SetLastBattleStats(_player, _enemy, true);
                _game.SetFlag("LastBattleOutcome", true);

                _pending = new StateTransition(
                    GameStateId.Victory,
                    new StateArgs { Reason = "BattleWon", Payload = new { xp, gold, enemyId = _enemy.Id } }
                );
            }
            else
            {
                _game.SetLastBattleStats(_player, _enemy, false);
                _game.SetFlag("LastBattleOutcome", false);

                _pending = new StateTransition(
                    GameStateId.Defeat,
                    new StateArgs { Reason = "BattleLost", Payload = new { enemyId = _enemy.Id } }
                );
            }

            _resolved = true;
            return Task.CompletedTask;
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

