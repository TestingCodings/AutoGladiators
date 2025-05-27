using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

public class BattlingState : IGameState
{
    public string Name => "Battling";

    public void Enter(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] enters battle.");
        bot.ResetBattleState();
    }

    public void Execute(GladiatorBot bot)
    {
        var result = BattleSimulator.Run(bot);
        bot.LastBattleResult = result;
        Console.WriteLine($"[{bot.Name}] battle outcome: {(result.Won ? "Victory" : "Defeat")}");
    }

    public void Exit(GladiatorBot bot)
    {
        Console.WriteLine($"[{bot.Name}] exits battle.");
    }
}
