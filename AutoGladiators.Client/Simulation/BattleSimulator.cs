using AutoGladiators.Client.Core;


namespace AutoGladiators.Client.Simulation

{
    public class BattleResult
    {
        public string Winner { get; set; }
        public List<string> Log { get; set; } = new();
    }

    public class BattleSimulator
    {
        public static BattleResult Run(GladiatorBot bot1, GladiatorBot bot2)
        {
            var result = new BattleResult();
            int round = 1;

            while (bot1.IsAlive && bot2.IsAlive && round <= 20)
            {
                result.Log.Add($"--- Round {round} ---");

                bot1.TakeTurn(bot2);
                bot2.TakeTurn(bot1);

                result.Log.Add($"{bot1.Name}: HP {bot1.Health}, Energy {bot1.Energy}, Last: {bot1.LastAction}");
                result.Log.Add($"{bot2.Name}: HP {bot2.Health}, Energy {bot2.Energy}, Last: {bot2.LastAction}");

                round++;
            }

            if (!bot1.IsAlive && !bot2.IsAlive)
                result.Winner = "Draw";
            else if (!bot1.IsAlive)
                result.Winner = bot2.Name;
            else if (!bot2.IsAlive)
                result.Winner = bot1.Name;
            else
                result.Winner = "Timeout";

            return result;
        }
    }
}
