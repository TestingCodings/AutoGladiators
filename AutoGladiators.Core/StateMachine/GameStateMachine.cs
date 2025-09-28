using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine.States;

namespace AutoGladiators.Core.StateMachine
{
    /// <summary>
    /// Minimal async state machine: current state runs ExecuteAsync and, when it returns a StateTransition,
    /// the machine performs Exit->Enter and continues.
    /// </summary>
    public sealed class GameStateMachine
    {
        private readonly IDictionary<GameStateId, IGameState> _states;
        private readonly GameStateContext _ctx;
        private IGameState? _current;

        public GameStateId? CurrentStateId => _current?.Id;
        public string CurrentStateName => _current?.Id.ToString() ?? "None";

        public GameStateMachine(IDictionary<GameStateId, IGameState> states, GameStateContext context)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
            _ctx = context ?? throw new ArgumentNullException(nameof(context));

            // Wire a callback so legacy code (ctx.TransitionTo(...)) can request transitions
            _ctx.RequestTransitionAsync = (nextId, args, ct) => TransitionToAsync(nextId, args, ct);
        }

        public async Task InitializeAsync(GameStateId initial, StateArgs? args = null, CancellationToken ct = default)
        {
            if (!_states.TryGetValue(initial, out var state))
                throw new KeyNotFoundException($"State {initial} not registered.");

            _current = state;
            await _current.EnterAsync(_ctx, args, ct);
        }

        public async Task TickAsync(CancellationToken ct = default)
        {
            if (_current is null) return;

            var trans = await _current.ExecuteAsync(_ctx, ct);
            if (trans is null) return;

            await TransitionToAsync(trans.Next, trans.Args, ct);
        }

        public async Task TransitionToAsync(GameStateId next, StateArgs? args = null, CancellationToken ct = default)
        {
            if (!_states.TryGetValue(next, out var target))
                throw new KeyNotFoundException($"State {next} not registered.");

            if (_current is not null)
                await _current.ExitAsync(_ctx, ct);

            _current = target;
            await _current.EnterAsync(_ctx, args, ct);
        }

        // Convenience for UI/actions that insist on strings (legacy)
        public Task ForceTransitionAsync(string to, StateArgs? args = null, CancellationToken ct = default)
        {
            if (!Enum.TryParse<GameStateId>(to, ignoreCase: true, out var id))
                throw new ArgumentException($"Unknown state name: {to}", nameof(to));

            return TransitionToAsync(id, args, ct);
        }

        // Synchronous methods for tests
        public void RequestTransition(GameStateId stateId)
        {
            // For tests, we'll do synchronous transitions
            TransitionToAsync(stateId).GetAwaiter().GetResult();
        }

        public void RequestTransition(StateTransition transition)
        {
            TransitionToAsync(transition.Next, transition.Args).GetAwaiter().GetResult();
        }

        public void SetState(GameStateId stateId)
        {
            // Force set state for tests
            if (!_states.TryGetValue(stateId, out var state))
                throw new KeyNotFoundException($"State {stateId} not registered.");
            _current = state;
        }
    }
}
