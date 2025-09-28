using NUnit.Framework;
using AutoGladiators.Core.Models;
using AutoGladiators.Tests.Utils;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Tests.Models
{
    [TestFixture]
    public class GladiatorBotTests
    {
        [Test]
        public void ApplyStatus_IncreasesStat()
        {
            var bot = TestBuilders.MakeBot(attack: 10);
            int effectiveStat = bot.GetEffectiveStat(bot.Strength, 5);
            Assert.That(effectiveStat, Is.GreaterThan(bot.Strength));
        }

        [Test]
        public void ReceiveDamage_KnocksOutAtZero()
        {
            var bot = TestBuilders.MakeBot(maxHp: 10);
            bot.ReceiveDamage(15);
            Assert.IsTrue(bot.IsFainted);
            Assert.That(bot.HP, Is.EqualTo(0));
        }


        [Test]
        public void LevelScaling_StatsIncreaseWithLevel()
        {
            var bot1 = TestBuilders.MakeBot(level: 1);
            var bot2 = TestBuilders.MakeBot(level: 10);
            Assert.That(bot2.Strength, Is.GreaterThan(bot1.Strength));
        }

        [Test]
        public void InventoryInteraction_OnlyIfNotKO()
        {
            var bot = TestBuilders.MakeBot(maxHp: 10);
            bot.ReceiveDamage(20);
            Assert.IsTrue(bot.IsFainted);
            // Test that healing doesn't work on fainted bots
            bot.Heal(10);
            Assert.That(bot.HP, Is.EqualTo(0)); // Should not heal KO'd bot
        }
    }
}
