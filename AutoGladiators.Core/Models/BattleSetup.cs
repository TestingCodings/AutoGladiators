using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.Models
{
    /// <summary>
    /// Configuration for setting up a battle between two bots
    /// </summary>
    public record BattleSetup(
        GladiatorBot PlayerBot,
        GladiatorBot EnemyBot,
        bool PlayerInitiated = true,
        string? BattleType = null,
        string? LocationId = null
    );
}