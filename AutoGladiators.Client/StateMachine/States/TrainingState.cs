using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;
public class TrainingState : IGameState
{
    public string Name => "Training";

    public void Enter(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] begins training...");
    }

    public void Execute(GladiatorBot bot)
    {
        bot.Train(); // Increase stats or XP
        Console.WriteLine($"[{bot.Name}] trained. New stats: STR={bot.Strength}, SPD={bot.Speed}, INT={bot.Intelligence}");
    }

    public void Exit(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] finishes training.");
    }
}
