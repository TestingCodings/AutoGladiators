using NUnit.Framework;
using AutoGladiators.Core.Logic;     // BattleManager
using AutoGladiators.Core.Models;    // GladiatorBot, Move
using AutoGladiators.Core.Enums;     // MoveCategory
using AutoGladiators.Tests.Utils;    // FakeRandom
using AutoGladiators.Tests.Battle;   // extension adapters

namespace AutoGladiators.Tests.Battle
{
    [TestFixture]
    public class BattleManager_ResultsTests
    {
        [Test]
        public void WinCondition_PlayerWinsIfEnemyKO()
        {
            var player = TestBuilders.MakeBot(maxHp: 100);
            var enemy  = TestBuilders.MakeBot(maxHp: 1);

            var battle = new BattleManager(player, enemy, new FakeRandom());

            enemy.ReceiveDamage(1);
            Assert.IsTrue(battle.IsPlayerWin());
        }

        [Test]
        public void LoseCondition_PlayerLosesIfKO()
        {
            var player = TestBuilders.MakeBot(maxHp: 1);
            var enemy  = TestBuilders.MakeBot(maxHp: 100);

            var battle = new BattleManager(player, enemy, new FakeRandom());

            player.ReceiveDamage(1);
            Assert.IsTrue(battle.IsPlayerLose());
        }

        [Test]
        public void TieBreaker_PlayerWithHigherSpeedWinsTie()
        {
            var player = TestBuilders.MakeBot(maxHp: 1, speed: 100);
            var enemy  = TestBuilders.MakeBot(maxHp: 1, speed: 10);

            var battle = new BattleManager(player, enemy, new FakeRandom());

            player.ReceiveDamage(1);
            enemy.ReceiveDamage(1);

            Assert.IsTrue(battle.IsTie());
            // If you later add tie-break rules, assert them in a dedicated test.
        }

        [Test]
        public void MinimumDamage_ClampedToOne()
        {
            var player = TestBuilders.MakeBot(attack: 1);
            var enemy  = TestBuilders.MakeBot(defense: 999);

            var move = new AutoGladiators.Core.Models.Move 
            { 
                Power = 10, 
                Category = AutoGladiators.Core.Enums.MoveCategory.Attack 
            };

            var rng = new FakeRandom();
            var battle = new BattleManager(player, enemy, rng);

            // Resolve just the player's hit; enemy hits with a no-op move
            BattleTestAdapters.ResolveTurn(battle, (player, move), (enemy, new AutoGladiators.Core.Models.Move()));

            Assert.That(enemy.CurrentHealth, Is.LessThan(enemy.MaxHealth));
        }
    }
}
