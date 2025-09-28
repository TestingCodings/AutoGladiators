using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Simulation;
using System.Diagnostics;

namespace AutoGladiators.Core.Integration
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
