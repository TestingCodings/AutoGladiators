namespace AutoGladiators.Client.StateMachine
{
    public interface IStateTransition
    {
        bool ShouldTransition();
    }
}
