using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Config;

namespace AutoGladiators.Client.Simulations
{
    public class BattleSimulator
    {
        public static string SimulateBattle(GladiatorBot bot1, GladiatorBot bot2)
        {
            var round = 1;
            var log = new List<string>();

            while (bot1.IsAlive && bot2.IsAlive && round <= 20)
            {
                log.Add($"--- Round {round} ---");

                bot1.TakeTurn(bot2);
                bot2.TakeTurn(bot1);

                log.Add($"{bot1.Name}: HP {bot1.Health}, Energy {bot1.Energy}, Last: {bot1.LastAction}");
                log.Add($"{bot2.Name}: HP {bot2.Health}, Energy {bot2.Energy}, Last: {bot2.LastAction}");

                round++;
            }

            if (!bot1.IsAlive && !bot2.IsAlive)
                return "It's a draw!\n" + string.Join("\n", log);

            if (!bot1.IsAlive)
                return $"{bot2.Name} wins!\n" + string.Join("\n", log);

            if (!bot2.IsAlive)
                return $"{bot1.Name} wins!\n" + string.Join("\n", log);

            return "Battle timed out.\n" + string.Join("\n", log);
        }
    }
}
