using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models.Items;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class CaptureServiceTests
    {
        private CaptureService _captureService = null!;
        
        [SetUp]
        public void Setup()
        {
            _captureService = new CaptureService();
        }
        
        [Test]
        public void AttemptCapture_WeakenedBot_HigherCaptureChance()
        {
            // Arrange
            var weakBot = new GladiatorBot
            {
                Name = "WeakBot",
                Level = 5,
                CurrentHealth = 10,
                MaxHealth = 100,
                AttackPower = 20,
                Defense = 15,
                Speed = 10
            };
            
            var captureDevice = CaptureGear.BasicNet;
            
            // Act
            var result = _captureService.AttemptCapture(weakBot, captureDevice, 10);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CaptureChance, Is.GreaterThan(0.1)); // Should have decent chance with weakened bot
            Assert.That(result.DeviceUsed, Is.EqualTo(captureDevice));
            Assert.That(result.Message, Is.Not.Empty);
        }
        
        [Test]
        public void AttemptCapture_FullHealthBot_LowerCaptureChance()
        {
            // Arrange
            var healthyBot = new GladiatorBot
            {
                Name = "HealthyBot",
                Level = 5,
                CurrentHealth = 100,
                MaxHealth = 100,
                AttackPower = 20,
                Defense = 15,
                Speed = 10
            };
            
            var captureDevice = CaptureGear.BasicNet;
            
            // Act
            var result = _captureService.AttemptCapture(healthyBot, captureDevice, 10);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CaptureChance, Is.LessThan(0.5)); // Should be harder with full health
        }
        
        [Test]
        public void AttemptCapture_WithBetterDevice_HigherChance()
        {
            // Arrange
            var bot = new GladiatorBot
            {
                Name = "TestBot",
                Level = 10,
                CurrentHealth = 50,
                MaxHealth = 100,
                AttackPower = 30,
                Defense = 25,
                Speed = 20
            };
            
            var basicNet = CaptureGear.BasicNet;
            var masterTrap = CaptureGear.MasterTrap;
            
            // Act
            var basicResult = _captureService.AttemptCapture(bot, basicNet, 10);
            var masterResult = _captureService.AttemptCapture(bot, masterTrap, 10);
            
            // Assert
            Assert.That(masterResult.CaptureChance, Is.GreaterThan(basicResult.CaptureChance));
        }
        
        [Test]
        public void GetRecommendedDevice_EmptyList_ReturnsBasicNet()
        {
            // Arrange
            var bot = new GladiatorBot { Name = "TestBot" };
            var emptyDevices = new List<CaptureGear>();
            
            // Act
            var recommended = _captureService.GetRecommendedDevice(bot, emptyDevices);
            
            // Assert
            Assert.That(recommended, Is.EqualTo(CaptureGear.BasicNet));
        }
        
        [Test]
        public void RequiresSpecialCapture_HighLevelBot_ReturnsTrue()
        {
            // Arrange
            var highLevelBot = new GladiatorBot
            {
                Name = "LegendaryBot",
                Level = 30,
                MaxHealth = 300,
                CurrentHealth = 300
            };
            
            // Act
            var requiresSpecial = _captureService.RequiresSpecialCapture(highLevelBot);
            
            // Assert
            Assert.That(requiresSpecial, Is.True);
        }
        
        [Test]
        public void AttemptCapture_NullBot_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _captureService.AttemptCapture(null!, CaptureGear.BasicNet, 10));
        }
        
        [Test]
        public void AttemptCapture_NullDevice_ThrowsException()
        {
            // Arrange
            var bot = new GladiatorBot { Name = "TestBot" };
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _captureService.AttemptCapture(bot, null!, 10));
        }
    }
}