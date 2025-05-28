using System;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;

namespace AutoGladiators.Client.StateMachine.States
{
    public class BattlingState : IGameState
    {
        public string Name => "Battling";

        public void Enter(GladiatorBot bot)
        {
            if (bot == null) return;

            Console.WriteLine($"\n=== {bot.Name} enters the battlefield ===");

            // TEMP: Create a placeholder opponent
            var opponent = new GladiatorBot("DummyBot", bot.Behavior, 100, 50);

            var result = BattleSimulator.SimulateBattle(bot, opponent);
            bot.LastBattleResult = result;

            Console.WriteLine($"\n🏆 Result: {result.Outcome} — Winner: {result.Winner}\n");

            foreach (var line in result.Log)
                Console.WriteLine($"• {line}");
        }

        public void Execute(GladiatorBot bot)
        {
            // Placeholder for real-time battle updates, AI decisions, etc.
        }

        public void Exit(GladiatorBot bot)
        {
            Console.WriteLine($"{bot.Name} leaves the battlefield.");
        }
    }
}
