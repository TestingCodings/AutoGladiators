using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;


namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class ExploringState : IGameState
    {
        public GameStateId Id => GameStateId.Exploring;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            var locationId = ctx.Game.GetPlayerLocationId();
            var location = MapService.Get(locationId);

            Console.WriteLine($"{ctx.Game.PlayerProfile?.Name ?? "Player"} begins exploring {location.Name}...");
            ctx.Ui?.ShowOverworld();
            ctx.Ui?.SetStatus($"Exploring {location.Name}…");

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            var locationId = ctx.Game.GetPlayerLocationId();
            var location = MapService.Get(locationId);

            // 1) Wild encounter
            if (EncounterGenerator.TryGenerateWildEncounter(location, out var wildBot))
            {
                Console.WriteLine($"Encountered wild {wildBot.Name}!");

                var playerBot = ctx.Game.GetCurrentBot();
                if (playerBot is not null)
                {
                    // Store encounter in GameStateService
                    ctx.Game.CurrentEncounter = wildBot;

                    return Task.FromResult<StateTransition?>(new StateTransition(
                        GameStateId.Battling,
                        new StateArgs { Reason = "RandomEncounter" }
                    ));
                }

                Console.WriteLine("No active bot found; skipping battle.");
            }

            // 2) NPC dialogue
            if (!string.IsNullOrEmpty(location.NPCId))
            {
                Console.WriteLine($"You meet NPC {location.NPCId}.");
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Dialogue,
                    new StateArgs { Reason = "NpcDialogue", Payload = location.NPCId }
                ));
            }

            Console.WriteLine($"{ctx.Game.PlayerProfile?.Name ?? "Player"} explores but nothing happens.");
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            var locationId = ctx.Game.GetPlayerLocationId();
            Console.WriteLine($"{ctx.Game.PlayerProfile?.Name ?? "Player"} finishes exploring {locationId}.");
            ctx.Ui?.HideOverworld();
            return Task.CompletedTask;
        }
    }
}