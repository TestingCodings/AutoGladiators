namespace AutoGladiators.Client.StateMachine
{
    /// <summary>
    /// Returned by a state when it wants to move to another state.
    /// </summary>
    public record StateTransition(GameStateId Next, StateArgs? Args = null);
}
