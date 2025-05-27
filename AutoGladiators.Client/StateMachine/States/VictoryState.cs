using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class VictoryState : IGameState
    {
        public string Name => "Victory";

        public void Enter(GladiatorBot bot)
        {
            bot.LevelUp();
            Console.WriteLine($"[{bot.Name}] achieved VICTORY! XP: {bot.Experience}, Level: {bot.Level}");
        }

        public void Execute(GladiatorBot bot)
        {
            Console.WriteLine($"[{bot.Name}] is celebrating victory...");
        }

        public void Exit(GladiatorBot bot)
        {
            Console.WriteLine($"[{bot.Name}] ends victory sequence.");
        }
    }
}