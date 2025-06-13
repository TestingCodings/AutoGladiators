using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Simulation
{
    public class SimulationResult
    {
        public List<string> Log { get; set; } = new();
        public GladiatorBot? Winner { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public GladiatorBot? CapturedBot { get; set; } = null;
        /// <summary>
        /// Initializes a new instance of the SimulationResult class with the specified winner and outcome.
        /// </summary>
        public SimulationResult(GladiatorBot? winner, string outcome)
        {
            Winner = winner;
            Outcome = outcome;
        }


        /// <summary>
        /// Initializes a new instance of the SimulationResult class with default values.
        /// </summary>
        public SimulationResult()
        {
        }

        /// <summary>
        /// Returns true if the simulation resulted in a meaningful outcome.
        /// </summary>
        public bool Success => !string.IsNullOrWhiteSpace(Outcome);

        /// <summary>
        /// Provides a short summary of the simulation, including the winner and final outcome.
        /// </summary>
        public string Summary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Outcome) && Winner == null)
                    return "Simulation incomplete or invalid.";
                return $"{Outcome} - Winner: {Winner?.Name ?? "None"}";
            }
        }

        /// <summary>
        /// Adds an event message to the simulation log.
        /// </summary>
        public void AddLog(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Log.Add(message);
        }

        /// <summary>
        /// Adds multiple entries to the simulation log.
        /// </summary>
        public void AddLogs(IEnumerable<string> messages)
        {
            if (messages != null)
                Log.AddRange(messages.Where(msg => !string.IsNullOrWhiteSpace(msg)));
        }

        /// <summary>
        /// Declares the winner of the simulation and sets the outcome.
        /// </summary>
        public void SetWinner(GladiatorBot bot)
        {
            Winner = bot;
            Outcome = $"{bot.Name} emerged victorious!";
        }

        /// <summary>
        /// Manually sets the simulation outcome (used for draws, escapes, etc.).
        /// </summary>
        public void SetOutcome(string outcome)
        {
            Outcome = outcome;
        }

        /// <summary>
        /// Resets the simulation result for reuse.
        /// </summary>
        public void Reset()
        {
            Log.Clear();
            Winner = null;
            Outcome = string.Empty;
        }
    }
}
// This class encapsulates the result of a simulation, including the winner, outcome, and log of events.
// It provides methods to add log entries, set the winner, and reset the simulation state for reuse.