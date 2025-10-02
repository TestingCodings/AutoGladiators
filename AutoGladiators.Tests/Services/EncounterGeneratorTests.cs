using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Tests.Utils;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class EncounterGeneratorTests
    {
        private EncounterGenerator _generator;
        
        [SetUp]
        public void Setup()
        {
            _generator = new EncounterGenerator();
        }
        
        [Test]
        public void GenerateWildEncounter_ValidLocation_ReturnsBot()
        {
            // Act
            var enemy = _generator.GenerateWildEncounter("Wilds");
            
            // Assert
            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(enemy.MaxHealth, Is.GreaterThan(0));
            Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
            Assert.That(enemy.Moveset, Is.Not.Null);
            Assert.That(enemy.Moveset.Count, Is.GreaterThan(0));
        }
        
        [Test]
        public void GenerateWildEncounter_GeneratesKnownEnemyTypes()
        {
            // Arrange
            var knownEnemies = new[] { "Scrapling", "Rustbeast", "Voltaic Drone" };
            var generatedEnemies = new List<string>();
            
            // Act - Generate multiple enemies to test variety
            for (int i = 0; i < 20; i++)
            {
                var enemy = _generator.GenerateWildEncounter("Wilds");
                if (enemy != null && !generatedEnemies.Contains(enemy.Name))
                {
                    generatedEnemies.Add(enemy.Name);
                }
            }
            
            // Assert - Should generate at least one of the known enemy types
            Assert.That(generatedEnemies.Any(name => knownEnemies.Contains(name)), Is.True, 
                $"Generated enemies: {string.Join(", ", generatedEnemies)}");
        }
        
        [Test]
        public void GenerateWildEncounter_EnemyHasValidMoveset()
        {
            // Act
            var enemy = _generator.GenerateWildEncounter("TestZone");
            
            // Assert
            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.Moveset, Is.Not.Null);
            Assert.That(enemy.Moveset.Count, Is.GreaterThan(0));
            
            // Verify moves are valid (not empty strings)
            foreach (var moveName in enemy.Moveset)
            {
                Assert.That(moveName, Is.Not.Null.And.Not.Empty);
            }
        }
        
        [Test]
        public void GenerateWildEncounter_EnemyHasValidStats()
        {
            // Act
            var enemy = _generator.GenerateWildEncounter("TestZone");
            
            // Assert
            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.Level, Is.GreaterThan(0));
            Assert.That(enemy.AttackPower, Is.GreaterThan(0));
            Assert.That(enemy.Defense, Is.GreaterThan(0));
            Assert.That(enemy.Speed, Is.GreaterThan(0));
            Assert.That(enemy.MaxHealth, Is.GreaterThan(0));
            Assert.That(enemy.MaxEnergy, Is.GreaterThan(0));
        }
        
        [Test]
        public void GenerateWildEncounter_DifferentLocations_GeneratesEnemies()
        {
            // Act
            var enemy1 = _generator.GenerateWildEncounter("Forest");
            var enemy2 = _generator.GenerateWildEncounter("Desert");
            var enemy3 = _generator.GenerateWildEncounter("Cave");
            
            // Assert - All locations should generate valid enemies
            Assert.That(enemy1, Is.Not.Null);
            Assert.That(enemy2, Is.Not.Null);
            Assert.That(enemy3, Is.Not.Null);
        }
    }
}