using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class ExploringState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<ExploringState>();

        public GameStateId Id => GameStateId.Exploring;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            ctx.Ui?.ShowOverworld();
            ctx.Ui?.SetStatus("Exploringâ€¦");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Generate encounter using new rarity-based system
            var encounterGenerator = new EncounterGenerator();
            var currentLocation = GameStateService.Instance.CurrentPlayer?.CurrentLocation?.Region ?? "Wilds";
            var wildBot = encounterGenerator.GenerateWildEncounter(currentLocation);
            
            if (wildBot != null)
            {
                GameStateService.Instance.CurrentEncounter = wildBot;
                Log.Info($"Encounter! Wild {wildBot.Name} (Lv{wildBot.Level}, {wildBot.Rarity}) appeared.");
                return Task.FromResult<StateTransition?>(new StateTransition(GameStateId.Battling));
            }
            
            Log.Info("No encounter this step. Continue exploring.");
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideOverworld();
            return Task.CompletedTask;
        }
    }
}

