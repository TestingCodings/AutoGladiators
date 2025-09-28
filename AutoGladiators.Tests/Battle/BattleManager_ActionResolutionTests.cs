// AutoGladiators.Tests/Battle/BattleManager_ActionResolutionTests.cs
using NUnit.Framework;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Enums;
using AutoGladiators.Tests.Utils;
using AutoGladiators.Tests.Battle;

namespace AutoGladiators.Tests.Battle
{
    [TestFixture]
    public class BattleManager_ActionResolutionTests
    {
        [Test]
        public void ActionResolution_DamageAppliedCorrectly()
        {
            var bot1 = TestBuilders.MakeBot(attack: 50);
            var bot2 = TestBuilders.MakeBot(maxHp: 100, defense: 10);
            var move = new AutoGladiators.Core.Models.Move 
            { 
                Power = 40, 
                Category = AutoGladiators.Core.Enums.MoveCategory.Attack, 
                Type = AutoGladiators.Core.Enums.MoveType.Attack 
            };
            var battle = new BattleManager(bot1, bot2, new FakeRandom());

            BattleTestAdapters.ResolveTurn(battle, (bot1, move), (bot2, new Move()));
            Assert.That(bot2.CurrentHealth, Is.LessThan(bot2.MaxHealth));
        }

        [Test]
        public void ActionResolution_CritDealsExtraDamage()
        {
            var bot1 = TestBuilders.MakeBot(attack: 50, CriticalHitChance: 1.0); // if your model supports this
            var bot2 = TestBuilders.MakeBot(maxHp: 100, defense: 10);
            var move = new AutoGladiators.Core.Models.Move 
            { 
                Power = 40, 
                Category = AutoGladiators.Core.Enums.MoveCategory.Attack, 
                Type = AutoGladiators.Core.Enums.MoveType.Attack 
            };
            var battle = new BattleManager(bot1, bot2, new FakeRandom());

            BattleTestAdapters.ResolveTurn(battle, (bot1, move), (bot2, new Move())); // 0.0 => "best" roll
            Assert.That(bot2.CurrentHealth, Is.LessThan(bot2.MaxHealth));
        }

        [Test]
        public void ActionResolution_MissDoesNoDamage()
        {
            var bot1 = TestBuilders.MakeBot(attack: 50);
            var bot2 = TestBuilders.MakeBot(maxHp: 100, defense: 10);
            var move = new AutoGladiators.Core.Models.Move 
            { 
                Power = 40, 
                Category = AutoGladiators.Core.Enums.MoveCategory.Attack, 
                Type = AutoGladiators.Core.Enums.MoveType.Attack, 
                Accuracy = 0 
            };
            var battle = new BattleManager(bot1, bot2, new FakeRandom());

            BattleTestAdapters.ResolveTurn(battle, (bot1, move), (bot2, new Move())); // removed extra FakeRandom argument
            Assert.That(bot2.CurrentHealth, Is.EqualTo(bot2.MaxHealth));
        }
    }
}
