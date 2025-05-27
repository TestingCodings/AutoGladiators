using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

public interface IGameState
{
    string Name { get; }
    void Enter(GladiatorBot bot);
    void Execute(GladiatorBot bot);
    void Exit(GladiatorBot bot);
}
