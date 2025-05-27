using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{

    public class DefeatState : IGameState
    {
        public string Name => "Defeat";

        public void Enter(GladiatorBot bot)
        {
            Console.WriteLine($"[{bot.Name}] has been DEFEATED.");
            bot.RecordDefeat();
        }

        public void Execute(GladiatorBot bot)
        {
            Console.WriteLine($"[{bot.Name}] is recovering...");
            bot.Health = Math.Max(bot.Health - 10, 0); // simulate cost
        }

        public void Exit(GladiatorBot bot)
        {
            Console.WriteLine($"[{bot.Name}] exits defeat and prepares to restart.");
        }
    }
}