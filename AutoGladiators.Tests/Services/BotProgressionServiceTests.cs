using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class BotProgressionServiceTests
    {
        private BotProgressionService _progressionService;
        private GladiatorBot _testBot;

        [SetUp]
        public void Setup()
        {
            _progressionService = new BotProgressionService();
            _testBot = new GladiatorBot
            {
                Id = 1,
                Name = "Test Bot",
                Level = 1,
                Experience = 0,
                MaxHealth = 100,
                CurrentHealth = 100,
                AttackPower = 20,
                Defense = 15,
                Speed = 10,
                MaxEnergy = 50,
                Energy = 50,
                Luck = 5,
                ElementalCore = ElementalCore.Fire
            };
        }

        [Test]
        public void GetXpRequirement_Level1_Returns100()
        {
            // Arrange & Act
            int requirement = _progressionService.GetXpRequirement(1);

            // Assert
            Assert.AreEqual(100, requirement);
        }

        [Test]
        public void GetXpRequirement_Level2_Returns125()
        {
            // Arrange & Act
            int requirement = _progressionService.GetXpRequirement(2);

            // Assert
            Assert.AreEqual(125, requirement); // 100 * 1.25
        }

        [Test]
        public void TryLevelUp_NotEnoughXp_NoLevelUp()
        {
            // Arrange
            _testBot.Experience = 50; // Less than 100 required

            // Act
            var result = _progressionService.TryLevelUp(_testBot);

            // Assert
            Assert.IsFalse(result.HasLeveledUp);
            Assert.AreEqual(0, result.LevelsGained);
            Assert.AreEqual(1, _testBot.Level);
        }

        [Test]
        public void TryLevelUp_ExactXp_LevelsUp()
        {
            // Arrange
            _testBot.Experience = 100; // Exact requirement

            // Act
            var result = _progressionService.TryLevelUp(_testBot);

            // Assert
            Assert.IsTrue(result.HasLeveledUp);
            Assert.AreEqual(1, result.LevelsGained);
            Assert.AreEqual(2, _testBot.Level);
            Assert.AreEqual(0, _testBot.Experience); // Should be consumed
        }

        [Test]
        public void TryLevelUp_Multiplelevels_LevelsUpMultipleTimes()
        {
            // Arrange
            _testBot.Experience = 250; // Enough for level 1->2 (100) and 2->3 (125)

            // Act
            var result = _progressionService.TryLevelUp(_testBot);

            // Assert
            Assert.IsTrue(result.HasLeveledUp);
            Assert.AreEqual(2, result.LevelsGained);
            Assert.AreEqual(3, _testBot.Level);
            Assert.AreEqual(25, _testBot.Experience); // 250 - 100 - 125 = 25 remaining
        }

        [Test]
        public void TryLevelUp_FireElemental_CorrectStatGrowth()
        {
            // Arrange
            _testBot.Experience = 100;
            _testBot.ElementalCore = ElementalCore.Fire;
            int initialHealth = _testBot.MaxHealth;
            int initialAttack = _testBot.AttackPower;

            // Act
            var result = _progressionService.TryLevelUp(_testBot);

            // Assert
            Assert.IsTrue(result.HasLeveledUp);
            Assert.IsTrue(_testBot.MaxHealth > initialHealth);
            Assert.IsTrue(_testBot.AttackPower > initialAttack);
            Assert.AreEqual(_testBot.MaxHealth, _testBot.CurrentHealth, "CurrentHealth should equal MaxHealth after level up");
        }

        [Test]
        public void CalculateStatGrowth_FireElement_ReturnsCorrectGrowth()
        {
            // Arrange
            _testBot.ElementalCore = ElementalCore.Fire;

            // Act
            var growth = _progressionService.CalculateStatGrowth(_testBot, 1);

            // Assert
            Assert.AreEqual(8, growth.Health); // Base fire growth
            Assert.AreEqual(4, growth.AttackPower);
            Assert.AreEqual(2, growth.Defense);
            Assert.AreEqual(3, growth.Speed);
            Assert.AreEqual(3, growth.Energy);
        }

        [Test]
        public void GetLevelProgress_HalfwayToNextLevel_ReturnsHalf()
        {
            // Arrange
            _testBot.Level = 1;
            _testBot.Experience = 50; // Half of 100 required

            // Act
            double progress = _progressionService.GetLevelProgress(_testBot);

            // Assert
            Assert.AreEqual(0.5, progress, 0.01);
        }
    }
}