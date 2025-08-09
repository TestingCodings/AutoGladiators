using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Logic;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.Core
{
    /// <summary>
    /// Central runtime: builds state machine, owns the tick loop.
    /// </summary>
    public sealed class GameRuntime
    {
        public static GameRuntime Instance { get; } = new();

        private GameRuntime() { }

        public GameStateMachine? Machine { get; private set; }
        public GameStateContext? Ctx { get; private set; }
        public CancellationTokenSource? LoopCts { get; private set; }

        /// <summary>
        /// Call once at app startup. You can pass a real UI bridge later.
        /// </summary>
        public async Task InitializeAsync(IUiBridge? ui = null, IMessageBus? bus = null, CancellationToken ct = default)
        {
            if (Machine is not null) return; // already init

            // Services (use your own singletons here)
            var game = GameStateService.Instance;
            var battleManager = new BattleManager(
                game.GetCurrentBot(),
                game.CurrentEncounter
            );


            // Build the shared context (remove bus/ui if you don’t use them)
            Ctx = new GameStateContext(game, battleManager, bus, ui);

            // Register states once
            var states = new Dictionary<GameStateId, IGameState>
            {
                { GameStateId.Idle,       new IdleState() },
                { GameStateId.Exploring,  new ExploringState() },
                { GameStateId.Dialogue,   new DialogueState() },
                { GameStateId.Battling,   new BattlingState() },
                { GameStateId.Inventory,  new InventoryState() },
                { GameStateId.Victory,    new VictoryState() },
                { GameStateId.Defeat,     new DefeatState() },
                { GameStateId.Fusion,     new FusionState() },
                { GameStateId.Training,   new TrainingState() },
                { GameStateId.GameOver, new GameOverState() },
            };

            Machine = new GameStateMachine(states, Ctx);

            // Start in Idle (or Exploring if you want)
            await Machine.InitializeAsync(GameStateId.Idle, ct: ct);
        }

        /// <summary>
        /// Start the tick loop (call from App startup). Call StopAsync to stop.
        /// </summary>
        public Task StartAsync(TimeSpan? tickInterval = null, CancellationToken externalCt = default)
        {
            if (Machine is null || Ctx is null)
                throw new InvalidOperationException("InitializeAsync must be called first.");

            if (LoopCts is not null)
                return Task.CompletedTask; // already running

            LoopCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            var ct = LoopCts.Token;
            var interval = tickInterval ?? TimeSpan.FromMilliseconds(100); // ~10 ticks/sec

            return Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await Machine.TickAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Game loop tick error: {ex}");
                    }
                    await Task.Delay(interval, ct);
                }
            }, ct);
        }

        public Task StopAsync()
        {
            LoopCts?.Cancel();
            LoopCts = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Force a transition by name (legacy UI).
        /// </summary>
        public Task GoToAsync(string stateName, StateArgs? args = null, CancellationToken ct = default)
            => Machine?.ForceTransitionAsync(stateName, args, ct) ?? Task.CompletedTask;

        /// <summary>
        /// Preferred: force a transition by enum.
        /// </summary>
        public Task GoToAsync(GameStateId id, StateArgs? args = null, CancellationToken ct = default)
            => Machine?.TransitionToAsync(id, args, ct) ?? Task.CompletedTask;
    }
}
    
