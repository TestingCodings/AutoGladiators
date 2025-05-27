using AutoGladiators.Client.Core;

public interface IGameState
{
    string Name { get; }
    void Enter(GladiatorBot bot);
    void Execute(GladiatorBot bot);
    void Exit(GladiatorBot bot);
}
