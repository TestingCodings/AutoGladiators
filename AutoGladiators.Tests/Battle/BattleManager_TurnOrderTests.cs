using NUnit.Framework;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Enums;
using AutoGladiators.Tests.Utils;
using AutoGladiators.Tests.Battle;

namespace AutoGladiators.Tests.Battle
{
    [TestFixture]
    public class BattleManager_TurnOrderTests
    {
        [Test]
        public void TurnOrder_HigherSpeedGoesFirst()
        {
            var bot1 = TestBuilders.MakeBot(name: "Fast", speed: 100);
            var bot2 = TestBuilders.MakeBot(name: "Slow", speed: 10);
            var battle = new BattleManager(bot1, bot2, new FakeRandom());
            var order = battle.DetermineTurnOrder();
            Assert.That(order[0], Is.EqualTo(bot1));
        }

        [Test]
        public void TurnOrder_PriorityOverridesSpeed()
        {
            var bot1 = TestBuilders.MakeBot(name: "Fast", speed: 100);
            var bot2 = TestBuilders.MakeBot(name: "Slow", speed: 10);
            var move1 = new AutoGladiators.Core.Models.Move { Priority = 1 };
            var move2 = new AutoGladiators.Core.Models.Move { Priority = 0 };
            var battle = new BattleManager(bot1, bot2, new FakeRandom());
            var order = battle.DetermineTurnOrder((bot1, move1), (bot2, move2));
            Assert.That(order[0].User, Is.EqualTo(bot1));
        }

        [Test]
        public void SimultaneousKO_ResultsInTie()
        {
            var bot1 = TestBuilders.MakeBot(maxHp: 1);
            var bot2 = TestBuilders.MakeBot(maxHp: 1);
            var battle = new BattleManager(bot1, bot2, new FakeRandom());
            bot1.ReceiveDamage(1);
            bot2.ReceiveDamage(1);
            Assert.IsTrue(battle.IsTie());
        }
    }
}
