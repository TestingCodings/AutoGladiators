using System.Collections.Generic;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.StateMachine.States;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Tests.StateMachine;

public static class StateMachineTestAdapters
{
    // Old tests: new GameStateMachine()
    public static GameStateMachine NewMachine(
        IDictionary<GameStateId, IGameState> states,
        GameStateContext context)
        => new(states, context);

    // Old: machine.SetState(id)
    public static void SetState(this GameStateMachine machine, GameStateId id)
        => machine.ForceTransitionAsync(id.ToString()); // if you have an internal, add InternalsVisibleTo or provide a wrapper in Core
                                      // If there's only a Transition API, queue a transition:
                                      // machine.QueueTransition(new StateTransition(id)); YOu can read the code copilot. YOu know what I have so implement it

    // Old: machine.RequestTransition(transition)
    public static void RequestTransition(this GameStateMachine machine, StateTransition transition)
        => machine.ForceTransitionAsync(transition.Next.ToString(), transition.Args);
}
