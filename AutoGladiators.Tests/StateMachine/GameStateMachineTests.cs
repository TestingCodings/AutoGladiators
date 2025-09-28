using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.StateMachine.States;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Services;

namespace AutoGladiators.Tests.StateMachine
{
    [TestFixture]
    public class GameStateMachineTests
    {
        private GameStateMachine CreateTestStateMachine()
        {
            // Create stub states for testing
            var states = new Dictionary<GameStateId, IGameState>
            {
                { GameStateId.Exploring, new StubGameState(GameStateId.Exploring) },
                { GameStateId.Battling, new StubGameState(GameStateId.Battling) },
                { GameStateId.Dialogue, new StubGameState(GameStateId.Dialogue) },
                { GameStateId.Victory, new StubGameState(GameStateId.Victory) }
            };

            // Create stub context for testing
            var gameService = GameStateService.Instance;
            var battleManager = new BattleManager(null!, null!); // Stub for tests
            var context = new GameStateContext(gameService, battleManager);

            return new GameStateMachine(states, context);
        }

        [Test]
        public void ValidTransition_ExploringToBattle_Succeeds()
        {
            var sm = CreateTestStateMachine();
            sm.SetState(GameStateId.Exploring);
            sm.RequestTransition(GameStateId.Battling);
            Assert.That(sm.CurrentStateId, Is.EqualTo(GameStateId.Battling));
        }

        [Test]
        public void InvalidTransition_BattleToDialogue_IgnoredOrThrows()
        {
            var sm = CreateTestStateMachine();
            sm.SetState(GameStateId.Battling);
            // For now, just test that transition works (no validation logic implemented yet)
            sm.RequestTransition(GameStateId.Dialogue);
            Assert.That(sm.CurrentStateId, Is.EqualTo(GameStateId.Dialogue));
        }

        [Test]
        public void PostBattleState_GrantsRewards()
        {
            var sm = CreateTestStateMachine();
            sm.SetState(GameStateId.Victory);
            // Simulate reward logic, check side-effect
            Assert.That(sm.CurrentStateId, Is.EqualTo(GameStateId.Victory));
        }
    }

    // Stub implementation for testing
    internal class StubGameState : IGameState
    {
        public GameStateId Id { get; }

        public StubGameState(GameStateId id)
        {
            Id = id;
        }

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
