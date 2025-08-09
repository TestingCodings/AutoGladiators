using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Logic;
using System.Diagnostics;

namespace AutoGladiators.Client.StateMachine.States
{
    // Optional: if you have a setup payload, reuse this record (or swap to your own)
    public record BattleSetup(GladiatorBot PlayerBot, GladiatorBot EnemyBot, bool PlayerInitiated);

    public sealed class BattlingState : IGameState
    {
        private readonly GameStateService _gameStateService;
        private readonly BattleManager _battleManager;

        private bool _battleStarted;
        private bool _battleEnded;
        private bool _playerModeEnabled;
        private BattleSetup? _setup;

        public BattlingState()
        {
            _gameStateService = GameStateService.Instance;
            _battleManager = new BattleManager(_gameStateService);
        }

        // New interface member
        public GameStateId Id => GameStateId.Battling;

        public async Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _battleStarted = false;
            _battleEnded = false;
            _playerModeEnabled = false;

            Debug.WriteLine("Entering Battle State...");

            // Accept setup via args if provided, otherwise pull from service
            _setup = args?.Payload as BattleSetup;
            if (_setup is null)
            {
                var playerBot = _gameStateService.GetCurrentBot();
                var enemyBot = _gameStateService.GetEncounteredBot();
                _setup = (playerBot != null && enemyBot != null)
                    ? new BattleSetup(playerBot, enemyBot, PlayerInitiated: true)
                    : null;
            }

            await StartBattleAsync(ct);
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (!_battleStarted || _battleEnded)
                return null;

            if (_battleManager.IsWaitingForPlayerInput)
            {
                // UI should call: _battleManager.SetSelectedMove(...) then next tick continues
                return null;
            }

            await _battleManager.ResolveTurnAsync(ct);

            if (_battleManager.BattleIsOver)
            {
                _battleEnded = true;
                return await HandleBattleResultAsync(ctx, ct);
            }

            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Debug.WriteLine("Exiting Battle State...");
            return Task.CompletedTask;
        }

        // ---- private helpers ----

        private async Task StartBattleAsync(CancellationToken ct)
        {
            if (_battleStarted) return;

            if (_setup is null || _setup.PlayerBot is null || _setup.EnemyBot is null)
            {
                Debug.WriteLine("Battle cannot start: missing bot(s)");
                _battleEnded = true;
                return;
            }

            _battleStarted = true;

            _battleManager.Initialize(_setup.PlayerBot, _setup.EnemyBot);
            await _battleManager.BeginTurnAsync(ct);
        }

        private async Task<StateTransition?> HandleBattleResultAsync(GameStateContext ctx, CancellationToken ct)
        {
            if (_battleManager.PlayerWon)
            {
                Debug.WriteLine("Player won the battle!");
                var rewards = _battleManager.GenerateBattleRewards();
                _gameStateService.ApplyBattleRewards(rewards);

                _gameStateService.SetFlag("LastBattleOutcome", "Win");
                _gameStateService.SetLastBattleStats(_battleManager.BattleLog);

                // If your machine uses IDs:
                return new StateTransition(GameStateId.PostBattle, new StateArgs
                {
                    Reason = "BattleWon",
                    Payload = rewards
                });
            }

            // Loss path with one-time “player mode” quirk
            if (!_playerModeEnabled)
            {
                Debug.WriteLine("Bot lost. Player enters combat themselves!");
                _playerModeEnabled = true;
                _battleManager.SwitchToPlayerMode();
                await _battleManager.BeginTurnAsync(ct);
                return null; // stay in BattlingState, continue the fight
            }

            // Final loss → GameOver
            Debug.WriteLine("Player defeated. Game Over.");
            _gameStateService.SetFlag("LastBattleOutcome", "Loss");
            _gameStateService.SetLastBattleStats(_battleManager.BattleLog);

            return new StateTransition(GameStateId.GameOver, new StateArgs
            {
                Reason = "BattleLost"
            });
        }
    }
}



