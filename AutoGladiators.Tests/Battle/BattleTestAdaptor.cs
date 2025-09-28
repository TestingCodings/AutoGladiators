using AutoGladiators.Core.Rng;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Tests.Battle;

public static class BattleTestAdapters
{
    // Old: new BattleManager(a, b, rng)
    // New: just create one (rng is passed to Move.Use later)
    public static BattleManager CreateManager(GladiatorBot player, GladiatorBot enemy, object? rng = null)
        => new(player, enemy, (IRng?)rng);

    // Old: bm.DetermineTurnOrder(aChoice, bChoice)
    public static (GladiatorBot User, Move Move)[] DetermineTurnOrder(
        this BattleManager bm,
        (GladiatorBot User, Move Move) a,
        (GladiatorBot User, Move Move) b)
        => bm.DetermineTurnOrder(a, b);

    // Old: bm.ResolveTurn(aChoice, bChoice, rng)
    public static void ResolveTurn(
        BattleManager battle,
        (GladiatorBot, Move) playerAction,
        (GladiatorBot, Move) enemyAction)
    {
        // Use the BattleManager's ResolveTurn method to ensure proper RNG usage
        battle.ResolveTurn(playerAction.Item2, enemyAction.Item2);
    }
    

    // Old: bm.IsTie(), IsPlayerWin(), IsPlayerLose()
    public static bool IsTie(this BattleManager bm, GladiatorBot p1, GladiatorBot p2)
        => p1.IsFainted && p2.IsFainted;

    public static bool IsPlayerWin(this BattleManager bm, GladiatorBot player, GladiatorBot enemy)
        => enemy.IsFainted && !player.IsFainted;

    public static bool IsPlayerLose(this BattleManager bm, GladiatorBot player, GladiatorBot enemy)
        => player.IsFainted && !enemy.IsFainted;
}
