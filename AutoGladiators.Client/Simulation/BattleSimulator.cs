using System.Collections.Generic;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Simulation
{
    public class BattleSimulator
    {
        public static BattleResult SimulateBattle(GladiatorBot bot1, GladiatorBot bot2)
        {
            var round = 1;
            var result = new BattleResult();

            result.BotStats[bot1.Name] = new Dictionary<string, int>
            {
                { "TurnsDefended", 0 },
                { "TurnsCharged", 0 },
                { "TotalDamageDealt", 0 },
            };

            result.BotStats[bot2.Name] = new Dictionary<string, int>
            {
                { "TurnsDefended", 0 },
                { "TurnsCharged", 0 },
                { "TotalDamageDealt", 0 },
            };

            while (bot1.IsAlive && bot2.IsAlive && round <= 20)
            {
                result.Log.Add($"--- Round {round} ---");

                var prevBot2Health = bot2.Health;
                var prevBot1Health = bot1.Health;

                bot1.TakeTurn(bot2);
                bot2.TakeTurn(bot1);

                result.BotStats[bot1.Name]["TotalDamageDealt"] += (prevBot2Health - bot2.Health);
                result.BotStats[bot2.Name]["TotalDamageDealt"] += (prevBot1Health - bot1.Health);

                if (bot1.LastAction == GladiatorAction.Defend)
                    result.BotStats[bot1.Name]["TurnsDefended"]++;
                if (bot2.LastAction == GladiatorAction.Defend)
                    result.BotStats[bot2.Name]["TurnsDefended"]++;

                if (bot1.LastAction == GladiatorAction.Charge)
                    result.BotStats[bot1.Name]["TurnsCharged"]++;
                if (bot2.LastAction == GladiatorAction.Charge)
                    result.BotStats[bot2.Name]["TurnsCharged"]++;

                result.Log.Add($"{bot1.Name}: HP {bot1.Health}, Energy {bot1.Energy}, Last: {bot1.LastAction}");
                result.Log.Add($"{bot2.Name}: HP {bot2.Health}, Energy {bot2.Energy}, Last: {bot2.LastAction}");

                round++;
            }

            result.TotalRounds = round - 1;
            result.FinalHealth[bot1.Name] = bot1.Health;
            result.FinalHealth[bot2.Name] = bot2.Health;
            result.LastActions[bot1.Name] = bot1.LastAction;
            result.LastActions[bot2.Name] = bot2.LastAction;

            if (!bot1.IsAlive && !bot2.IsAlive)
            {
                result.Winner = "None";
                result.Outcome = "Draw";
            }
            else if (!bot1.IsAlive)
            {
                result.Winner = bot2.Name;
                result.Outcome = "Loss";
            }
            else if (!bot2.IsAlive)
            {
                result.Winner = bot1.Name;
                result.Outcome = "Win";
            }
            else
            {
                result.Winner = "None";
                result.Outcome = "Timeout";
            }

            return result;
        }
    }
}