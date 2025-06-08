using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    public interface IGameState
    {
        string Name { get; }
        void Enter(GladiatorBot bot);
        void Execute(GladiatorBot bot);
        void Exit(GladiatorBot bot);
    }
}

