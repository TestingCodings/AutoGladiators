using System;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.Models
{
    /// <summary>
    /// Represents a selectable dialogue choice presented to the player during an NPC interaction.
    /// </summary>
    public class DialogueOption
    {
        /// <summary>
        /// The text shown to the player for this option.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the next DialogueNode to go to if this option is selected.
        /// </summary>
        public string? NextNodeId { get; set; }

        /// <summary>
        /// An optional in-game action to trigger (e.g., "GiveItem", "StartBattle", "HealBot").
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// A condition function that determines whether this option is available based on the player's profile.
        /// </summary>
        public Func<PlayerProfile, bool>? Condition { get; set; }

        /// <summary>
        /// Returns true if the condition is met or not defined (i.e., the option is available).
        /// </summary>
        public bool IsAvailable(PlayerProfile profile)
        {
            return Condition?.Invoke(profile) ?? true;
        }
    }
}
