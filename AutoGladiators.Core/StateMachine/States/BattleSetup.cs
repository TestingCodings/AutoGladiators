using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.StateMachine.States
{
    public record BattleSetup(GladiatorBot PlayerBot, GladiatorBot EnemyBot, bool PlayerInitiated);
}

