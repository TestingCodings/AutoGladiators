using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;
using AutoGladiators.Client.StateMachine.Transitions;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine
{
    public class GameStateMachine
    {
        private readonly Dictionary<(string, string), IStateTransition> _transitions = new();
        private IGameState _currentState;

        public GameStateMachine(IGameState initialState)
        {
            _currentState = initialState;
        }

        public void Initialize(GladiatorBot bot)
        {
            _currentState?.Enter(bot);
        }

        public void AddTransition(string from, string to, IStateTransition transition)
        {
            _transitions[(from, to)] = transition;
        }

        public void Update(GladiatorBot bot)
        {
            _currentState?.Execute(bot);

            // Assuming GladiatorBot implements IGameStateContext, cast it; otherwise, adapt as needed.
            var context = bot as IGameStateContext;
            if (context == null)
                throw new InvalidCastException("GladiatorBot must implement IGameStateContext.");

            foreach (var kvp in _transitions)
            {
                if (kvp.Key.Item1 == _currentState.Name && kvp.Value.CanTransition(context))
                {
                    _currentState?.Exit(bot);
                    _currentState = GameStateFactory.CreateState(kvp.Key.Item2);
                    _currentState.Enter(bot);
                    break;
                }
            }
        }

        public void ForceTransition(string to, GladiatorBot bot)
        {
            _currentState?.Exit(bot);
            _currentState = GameStateFactory.CreateState(to);
            _currentState.Enter(bot);
        }

        public string CurrentStateName => _currentState?.Name ?? "None";
    }
}

// This code defines a GameStateMachine that manages the state transitions of a GladiatorBot in a game.
// It allows adding transitions between states, updating the current state based on conditions, and forcing transitions.