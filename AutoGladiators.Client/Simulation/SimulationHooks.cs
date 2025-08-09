using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Simulation;
using System.Diagnostics;

namespace AutoGladiators.Client.Integration
{
    public static class SimulationHooks
    {

        public static void PreviewTraining(GladiatorBot bot, string skill)
        {
            var result = TrainingSimulator.SimulateTraining(bot, skill);
            Debug.WriteLine(result.Summary);
        }

        public static void TryBotCapture(GladiatorBot bot)
        {
            var result = CaptureSimulator.TryCapture(bot, 50);
            Debug.WriteLine(result.Summary);
        }
    }
}