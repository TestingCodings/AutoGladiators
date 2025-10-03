using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class InventoryServiceTests
    {
        private InventoryService _inventoryService;
        private GladiatorBot _testBot;

        [SetUp]
        public void Setup()
        {
            _inventoryService = InventoryService.Instance;
            _inventoryService.ClearInventory(); // Clear inventory state between tests
            _testBot = new GladiatorBot
            {
                Id = 1,
                Name = "Test Bot",
                Level = 1,
                MaxHealth = 100,
                CurrentHealth = 50, // Half health for testing healing
                MaxEnergy = 50,
                Energy = 25 // Half energy for testing
            };
        }

        [Test]
        public void HealingPotion_Use_RestoresHealth()
        {
            // Arrange
            var potion = new HealingPotion(30);
            int initialHealth = _testBot.CurrentHealth;

            // Act
            var result = potion.Use(_testBot);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialHealth + 30, _testBot.CurrentHealth);
            Assert.AreEqual(30, result.Value);
        }

        [Test]
        public void HealingPotion_UseOnFullHealth_Fails()
        {
            // Arrange
            var potion = new HealingPotion(30);
            _testBot.CurrentHealth = _testBot.MaxHealth; // Full health

            // Act
            var result = potion.Use(_testBot);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Bot is already at full health", result.Message);
        }

        [Test]
        public void HealingPotion_UseOverMaxHealth_CapsAtMax()
        {
            // Arrange
            var potion = new HealingPotion(100);
            _testBot.CurrentHealth = 90; // Close to max

            // Act
            var result = potion.Use(_testBot);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(_testBot.MaxHealth, _testBot.CurrentHealth);
            Assert.AreEqual(10, result.Value); // Only healed 10, not 100
        }

        [Test]
        public void EnergyPotion_Use_RestoresEnergy()
        {
            // Arrange
            var potion = new EnergyPotion(20);
            int initialEnergy = _testBot.Energy;

            // Act
            var result = potion.Use(_testBot);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialEnergy + 20, _testBot.Energy);
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public void EnergyPotion_UseOnFullEnergy_Fails()
        {
            // Arrange
            var potion = new EnergyPotion(20);
            _testBot.Energy = _testBot.MaxEnergy; // Full energy

            // Act
            var result = potion.Use(_testBot);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Bot is already at full energy", result.Message);
        }

        [Test]
        public void AddItem_NewItem_AddsToInventory()
        {
            // Arrange
            var potion = new HealingPotion(50);

            // Act
            _inventoryService.AddItem(potion, 2);

            // Assert
            var items = _inventoryService.GetItemsByType(AutoGladiators.Core.Services.ItemType.Healing);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("Healing Potion", items[0].Name);
        }

        [Test]
        public void UseModernItem_ValidItem_UsesAndRemoves()
        {
            // Arrange
            var potion = new HealingPotion(30);
            _inventoryService.AddItem(potion, 1);
            int initialHealth = _testBot.CurrentHealth;

            // Act
            var result = _inventoryService.UseModernItem(potion.Id, _testBot);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialHealth + 30, _testBot.CurrentHealth);
            
            // Item should be removed from inventory
            var remainingItems = _inventoryService.GetItemsByType(AutoGladiators.Core.Services.ItemType.Healing);
            Assert.AreEqual(0, remainingItems.Count);
        }

        [Test]
        public void StatBooster_AttackBoost_IncreasesAttack()
        {
            // Arrange
            var booster = new StatBooster(StatType.Attack, 5, true);
            int initialAttack = _testBot.AttackPower;

            // Act
            var result = booster.Use(_testBot);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialAttack + 5, _testBot.AttackPower);
            Assert.AreEqual(5, result.Value);
        }

        [Test]
        public void CaptureDevice_HighHealth_Fails()
        {
            // Arrange
            var captureDevice = new CaptureDevice(1.0); // 100% capture rate
            _testBot.CurrentHealth = _testBot.MaxHealth; // Full health

            // Act
            var result = captureDevice.Use(_testBot);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("must be weakened"));
        }

        [Test]
        public void CaptureDevice_LowHealth_CanAttemptCapture()
        {
            // Arrange
            var captureDevice = new CaptureDevice(1.0); // 100% capture rate
            _testBot.CurrentHealth = 5; // Very low health (5% of 100)

            // Act
            var result = captureDevice.Use(_testBot);

            // Assert
            Assert.IsTrue(result.Success);
            // With 100% base rate and very low health, should capture or at least attempt
            Assert.IsTrue(captureDevice.RemainingUses < captureDevice.MaxUses);
        }
    }
}