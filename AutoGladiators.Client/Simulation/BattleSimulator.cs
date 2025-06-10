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
            int bot1Score = bot1.Attack(bot2) + bot1.Defense(bot2) + bot1.Speed(bot2) + new Random().Next(0, 10);
            int bot2Score = bot2.Attack(bot1) + bot2.Defense(bot1) + bot2.Speed(bot1) + new Random().Next(0, 10);

            GladiatorBot winner = bot1Score >= bot2Score ? bot1 : bot2;
            return new SimulationResult
            {
                // If Summary is read-only, remove this line or set via constructor if possible
                Winner = winner
            };
        }
    }
}