using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using System.Text;

namespace AutoGladiators.Client.Simulation
{
    public static class BattleSimulator
    {
        public static SimulationResult SimulateBattle(GladiatorBot bot1, GladiatorBot bot2)
        {
            var battleLog = new StringBuilder();
            battleLog.AppendLine($"Battle Start: {bot1.Name} vs {bot2.Name}");

            int round = 1;

            // Determine the initial attacker based on speed
            GladiatorBot attacker = bot1.Speed >= bot2.Speed ? bot1 : bot2;
            GladiatorBot defender = attacker == bot1 ? bot2 : bot1;

            while (bot1.IsAlive && bot2.IsAlive && round <= 50)
            {
                string action = attacker.Attack(defender);
                if (string.IsNullOrEmpty(action))
                {
                    action = "No action performed.";
                }
                battleLog.AppendLine($"Round {round}: {action}");

                // Swap roles
                (attacker, defender) = (defender, attacker);
                round++;
            }

            var result = new SimulationResult();
            result.AddLog(battleLog.ToString());

            if (bot1.IsAlive && !bot2.IsAlive)
            {
                result.SetWinner(bot1);
            }
            else if (bot2.IsAlive && !bot1.IsAlive)
            {
                result.SetWinner(bot2);
            }
            else if (!bot1.IsAlive && !bot2.IsAlive)
            {
                result.SetOutcome("Both gladiators have fallen. No winner.");
            }
            else
            {
                result.SetOutcome("Battle reached round limit. It's a draw!");
            }

            return result;
        }
                public static SimulationResult Evade(GladiatorBot bot1, GladiatorBot bot2)
        {
            var result = new SimulationResult();

            result.AddLog($"{bot1.Name} attempts to evade!");
            bool success = new Random().NextDouble() < 0.5;

            if (success)
            {
                result.AddLog($"{bot1.Name} successfully evaded the attack!");
                result.SetOutcome($"{bot1.Name} dodged the fight.");
            }
            else
            {
                result.AddLog($"{bot1.Name} failed to evade and took a counter hit.");
                bot1.CurrentHealth -= 5;
                result.SetOutcome($"{bot1.Name} got hit while evading.");
            }

            return result;
        }

        public static SimulationResult PowerStrike(GladiatorBot bot1, GladiatorBot bot2)
        {
            var result = new SimulationResult();
            result.AddLog($"{bot1.Name} charges a Power Strike!");

            int baseDamage = 20;
            bot2.CurrentHealth -= baseDamage;

            result.AddLog($"{bot1.Name} hits {bot2.Name} for {baseDamage} damage!");

            if (!bot2.IsAlive)
            {
                result.SetWinner(bot1);
            }
            else
            {
                result.SetOutcome($"{bot2.Name} is still standing!");
            }

            return result;
        }

    }
}
