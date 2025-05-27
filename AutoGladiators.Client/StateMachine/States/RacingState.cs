using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;
public class RacingState : IGameState
{
    public string Name => "Racing";

    public void Enter(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] starts racing...");
    }

    public void Execute(GladiatorBot bot)
    {
        var result = RaceSimulator.Run(bot);
        bot.LastRaceResult = result;
        Console.WriteLine($"[{bot.Name}] finished race. Time: {result.TimeTaken}, Crashes: {result.Crashes}");
    }

    public void Exit(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] completed racing.");
    }
}
