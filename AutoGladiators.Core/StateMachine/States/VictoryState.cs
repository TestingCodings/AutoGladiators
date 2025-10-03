using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class VictoryState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<VictoryState>();

        public GameStateId Id => GameStateId.Victory;

        private bool _rewardsApplied = false;
        private bool _userConfirmed = false;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _rewardsApplied = false;
            _userConfirmed = false;
            
            Log.Info("Battle concluded. Victory!");
            
            // Extract reward information from payload
            var payload = args?.Payload as dynamic;
            var xp = payload?.xp ?? 0;
            var gold = payload?.gold ?? 0;
            var enemyId = payload?.enemyId ?? "";

            // Display victory screen with reward details
            var victoryData = new
            {
                xp = xp,
                gold = gold,
                enemyId = enemyId,
                message = $"Victory! You earned {xp} XP and {gold} Gold!"
            };

            ctx.Ui?.ShowVictoryScreen(victoryData);
            ctx.Ui?.SetStatus($"Victory! +{xp} XP, +{gold} Gold");

            _rewardsApplied = true;
            Log.Info($"Victory rewards applied: {xp} XP, {gold} Gold");
            
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // For now, automatically continue after a brief moment
            // In a real UI implementation, this would wait for user input
            if (_rewardsApplied && !_userConfirmed)
            {
                _userConfirmed = true;
                
                Log.Info("Returning to adventure after victory.");
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "VictoryReturn" }
                ));
            }

            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Console.WriteLine("Post-battle victory tasks completed.");
            ctx.Ui?.HideVictoryScreen();
            ctx.Ui?.SetStatus("Returned to adventure");
            
            // Reset state for next battle
            _rewardsApplied = false;
            _userConfirmed = false;
            
            return Task.CompletedTask;
        }
    }
}

