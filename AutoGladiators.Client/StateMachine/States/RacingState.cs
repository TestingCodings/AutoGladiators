using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;

namespace AutoGladiators.Client.StateMachine.States
{
    public class RacingState : IGameState
    {
        public string Name => "Racing";

        public void Enter(GladiatorBot bot)
        {
            Console.WriteLine($"{bot.Name} has entered the race track!");

            // TEMP: Simulate against 2 dummy bots for now
            var opponent1 = new GladiatorBot("ZoomBot", bot.Behavior, 100, 50);
            var opponent2 = new GladiatorBot("QuickBot", bot.Behavior, 100, 50);
            var participants = new List<GladiatorBot> { bot, opponent1, opponent2 };

            var result = new RaceSimulator().Run(participants);

            Console.WriteLine($"🏁 Winner: {result.Winner.Name}");
            foreach (var entry in result.Scores)
            {
                Console.WriteLine($"{entry.Key.Name}: Score = {entry.Value}");
            }
        }

        public void Execute(GladiatorBot bot)
        {
            // Future logic
        }

        public void Exit(GladiatorBot bot)
        {
            Console.WriteLine($"{bot.Name} exits the race.");
        }
    }
}
