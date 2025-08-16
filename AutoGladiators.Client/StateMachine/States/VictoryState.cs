using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class VictoryState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<VictoryState>();

        public GameStateId Id => GameStateId.Victory;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Console.WriteLine("Battle concluded. Victory!");

            // If battle rewards were passed in from BattlingState:
            // Replace 'object' with your actual rewards type, e.g. (int xp, int gold)
            var rewards = args?.Payload as (int xp, int gold)?;
            if (rewards != null)
            {
                // Apply rewards to player profile
                ctx.Game.ApplyBattleRewards(rewards.Value.xp, rewards.Value.gold);
            }

            // TODO: Handle XP gain, loot drops, achievements, etc.
            // ctx.Game.AwardExperience(...);
            // ctx.Game.AddLoot(...);

            ctx.Ui?.ShowVictoryScreen(rewards);

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // You could wait for a UI "continue" button here instead of auto-transitioning
            // For now, we auto-transition back to Exploring
            Console.WriteLine("Returning to adventure...");
            return Task.FromResult<StateTransition?>(new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "VictoryReturn" }
            ));
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Console.WriteLine("Post-battle tasks completed.");
            ctx.Ui?.HideVictoryScreen();
            return Task.CompletedTask;
        }
    }
}

