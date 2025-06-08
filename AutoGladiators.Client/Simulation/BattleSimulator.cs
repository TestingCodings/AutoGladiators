using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine;
using System;

namespace AutoGladiators.Client.Simulation
{
    public static class BattleSimulator
    {
        public static SimulationResult SimulateBattle(GladiatorBot bot1, GladiatorBot bot2)
        {
            int bot1Score = bot1.Attack + bot1.Defense + bot1.Speed + new Random().Next(0, 10);
            int bot2Score = bot2.Attack + bot2.Defense + bot2.Speed + new Random().Next(0, 10);

            string winner = bot1Score >= bot2Score ? bot1.Name : bot2.Name;
            return new SimulationResult
            {
                Summary = $"{bot1.Name} vs {bot2.Name} - Winner: {winner}",
                WinnerBot = winner
            };
        }
    }
}