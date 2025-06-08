namespace AutoGladiators.Client.StateMachine.Transitions
{
    public abstract class StateTransition
    {
        public abstract bool ShouldTransition(IGameStateContext context);
        public abstract IGameState GetNextState();
    }
}
