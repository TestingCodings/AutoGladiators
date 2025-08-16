using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    public record BattleSetup(GladiatorBot PlayerBot, GladiatorBot EnemyBot, bool PlayerInitiated);
}

