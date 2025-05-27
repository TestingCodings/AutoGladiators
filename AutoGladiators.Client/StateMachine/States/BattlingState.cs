using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using System;

namespace AutoGladiators.Client.StateMachine.States
{
    public class BattlingState : IGameState
    {
        public string Name => "Battling";

        public void Enter(GladiatorBot bot)
        {
            Console.WriteLine($"{bot.Name} has entered the battlefield!");

            // TEMP FIX: Use a dummy opponent until multi-bot injection is added
            var opponent = new GladiatorBot("DummyBot", bot.Behavior, 100, 50);
            var result = BattleSimulator.Run(bot, opponent);

            Console.WriteLine($"{result.Winner} won the battle!");
            foreach (var logLine in result.Log)
                Console.WriteLine(logLine);
        }

        public void Execute(GladiatorBot bot)
        {
            // Future combat loop logic
        }

        public void Exit(GladiatorBot bot)
        {
            Console.WriteLine($"{bot.Name} leaves the battle.");
        }
    }
}
