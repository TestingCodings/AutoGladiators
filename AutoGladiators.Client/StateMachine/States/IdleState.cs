
using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;
public class IdleState : IGameState
{
    public string Name => "Idle";

    public void Enter(GladiatorBot bot)
    {
        bot.ResetTempStats();
        Console.WriteLine($"[{bot.Name}] has entered Idle state.");
    }

    public void Execute(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] is waiting...");
        // Could check for decision to train, race, or battle
    }

    public void Exit(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] is exiting Idle state.");
    }
}
