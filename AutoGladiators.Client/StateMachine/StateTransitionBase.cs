using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public abstract class StateTransitionBase : IStateTransition
    {
        public abstract string Name { get; }
        public abstract IGameState TargetState { get; }

        // Must be implemented by subclasses
        public abstract bool ShouldTransition(GladiatorBot bot);

        // Optional override
        public virtual bool CanTransition(GladiatorBot bot) => true;

        /// <summary>
        /// Helper: Roll a chance-based probability.
        /// </summary>
        protected bool RollChance(double probability)
        {
            return new Random().NextDouble() <= probability;
        }

        /// <summary>
        /// Helper: Checks if bot has a required modifier.
        /// </summary>
        protected bool HasModifier(GladiatorBot bot, string modifier)
        {
            return bot.Modifiers.Contains(modifier);
        }

        /// <summary>
        /// Helper: Placeholder to simulate environment influence.
        /// replace this with a LevelConfig context or similar.
        /// </summary>
        protected bool IsEnvironment(string env, GladiatorBot bot)
        {
            return bot.LastRaceResult?.ToString().Contains(env) == true; // crude placeholder
        }
    }
}
