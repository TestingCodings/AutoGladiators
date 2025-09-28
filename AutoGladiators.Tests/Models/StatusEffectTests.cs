using NUnit.Framework;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Enums;
using AutoGladiators.Tests.Utils;

namespace AutoGladiators.Tests.Models
{
    [TestFixture]
    public class StatusEffectTests
    {
        [Test]
        public void ApplyStatusEffect_AddsEffect()
        {
            var bot = TestBuilders.MakeBot();
            var effect = new StatusEffect { Type = StatusEffectType.Burn, Duration = 3 };
            bot.ApplyStatusEffect(effect);
            Assert.IsTrue(bot.HasStatus(StatusEffectType.Burn));
        }

        [Test]
        public void StackingRules_RefreshesDuration()
        {
            var bot = TestBuilders.MakeBot();
            var effect = new StatusEffect { Type = StatusEffectType.Burn, Duration = 2 };
            bot.ApplyStatusEffect(effect);
            bot.ApplyStatusEffect(new StatusEffect { Type = StatusEffectType.Burn, Duration = 5 });
            Assert.That(bot.GetStatus(StatusEffectType.Burn)?.Duration, Is.EqualTo(5));
        }

        [Test]
        public void TickEffect_DecrementsDurationAndExpires()
        {
            var bot = TestBuilders.MakeBot();
            var effect = new StatusEffect { Type = StatusEffectType.Poison, Duration = 2 };
            bot.ApplyStatusEffect(effect);
            bot.TickStatusEffects();
            Assert.IsTrue(bot.HasStatus(StatusEffectType.Poison));
            bot.TickStatusEffects();
            Assert.IsFalse(bot.HasStatus(StatusEffectType.Poison));
        }

        [Test]
        public void Cleanse_RemovesAllEffects()
        {
            var bot = TestBuilders.MakeBot();
            bot.ApplyStatusEffect(new StatusEffect { Type = StatusEffectType.Burn, Duration = 2 });
            bot.ApplyStatusEffect(new StatusEffect { Type = StatusEffectType.Poison, Duration = 2 });
            bot.Cleanse();
            Assert.IsFalse(bot.HasStatus(StatusEffectType.Burn));
            Assert.IsFalse(bot.HasStatus(StatusEffectType.Poison));
        }
    }
}
