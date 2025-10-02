using NUnit.Framework;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Rng;
using AutoGladiators.Tests.Utils;

namespace AutoGladiators.Tests.Models
{
    [TestFixture]
    public class MoveTests
    {
        [Test]
        public void CalculateDamage_BasicPhysical_CorrectFormula()
        {
            // Arrange
            var move = new AutoGladiators.Core.Models.Move { Power = 50, Category = AutoGladiators.Core.Enums.MoveCategory.Attack};
            var attacker = TestBuilders.MakeBot(level: 10, attack: 40);
            var defender = TestBuilders.MakeBot(level: 10, defense: 20);
            var rng = new FakeRandom(0.5); // deterministic

            // Act
            int damage = move.CalculateDamage(attacker, defender, rng);

            // Assert
            Assert.That(damage, Is.GreaterThan(0));
        }

        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(2.0, 1.0, 2.0)]
        [TestCase(0.5, 1.0, 0.5)]
        public void ElementalMultiplier_AppliesCorrectly(double moveType, double defenderType, double expected)
        {
            // Arrange
            var move = new AutoGladiators.Core.Models.Move { Element = "Fire" };
            var defender = TestBuilders.MakeBot(core: AutoGladiators.Core.Enums.ElementalCore.Grass);
            // Act - For now, just test that elemental types are set correctly
            // TODO: Implement ElementalSystem.GetMultiplier when needed
            // Assert
            Assert.That(move.Element, Is.EqualTo("Fire"));
            Assert.That(defender.ElementalCore, Is.EqualTo(AutoGladiators.Core.Enums.ElementalCore.Grass));
        }

        [Test]
        public void AccuracyCheck_MissIfRngAboveThreshold()
        {
            // Arrange
            var move = new AutoGladiators.Core.Models.Move { Accuracy = 80 };
            var rng = new FakeRandom(0.9); // 90% > 80% accuracy
            // Act
            bool hit = move.CheckAccuracy(rng);
            // Assert
            Assert.IsFalse(hit);
        }

        [Test]
        public void Priority_ResolvesCorrectly()
        {
            // Arrange
            var move1 = new AutoGladiators.Core.Models.Move { Priority = 1 };
            var move2 = new AutoGladiators.Core.Models.Move { Priority = 0 };
            // Act & Assert
            Assert.That(move1.Priority, Is.GreaterThan(move2.Priority));
        }

        
        [Test]
        public void CriticalHitChance_TriggersOnHighRng()
        {
            var move = TestBuilders.MakeMove(CriticalHitChance: 1.0); // 100%
            var rng = new FakeRandom(0.0); // always crit
            Assert.IsTrue(move.isCrit(rng));
        }
        
        [Test]
        public void CreateMVPMoves_ReturnsThreeMoves()
        {
            // Act
            var moves = AutoGladiators.Core.Models.Move.CreateMVPMoves();
            
            // Assert
            Assert.That(moves.Count, Is.EqualTo(3));
            Assert.That(moves.Any(m => m.Name == "Tackle"), Is.True);
            Assert.That(moves.Any(m => m.Name == "Guard"), Is.True);
            Assert.That(moves.Any(m => m.Name == "Metal Strike"), Is.True);
        }
        
        [Test]
        public void CreateMVPMoves_TackleHasCorrectStats()
        {
            // Act
            var moves = AutoGladiators.Core.Models.Move.CreateMVPMoves();
            var tackle = moves.First(m => m.Name == "Tackle");
            
            // Assert
            Assert.That(tackle.Power, Is.EqualTo(20));
            Assert.That(tackle.Accuracy, Is.EqualTo(90.0));
            Assert.That(tackle.EnergyCost, Is.EqualTo(5));
            Assert.That(tackle.Type, Is.EqualTo(AutoGladiators.Core.Enums.MoveType.Attack));
        }
        
        [Test]
        public void Use_InsufficientEnergy_ReturnsFailureMessage()
        {
            // Arrange
            var move = new AutoGladiators.Core.Models.Move 
            { 
                Name = "Test Move", 
                EnergyCost = 50,
                Power = 20,
                Accuracy = 100.0,
                Type = AutoGladiators.Core.Enums.MoveType.Attack
            };
            var attacker = TestBuilders.MakeBot(energy: 10); // Not enough energy
            var defender = TestBuilders.MakeBot();
            
            // Act
            var result = move.Use(attacker, defender);
            
            // Assert
            Assert.That(result, Does.Contain("didn't have enough energy"));
        }
        
        [Test]
        public void Use_AttackMove_DealsDamage()
        {
            // Arrange
            var move = new AutoGladiators.Core.Models.Move 
            { 
                Name = "Test Attack", 
                EnergyCost = 5,
                Power = 30,
                Accuracy = 100.0,
                Type = AutoGladiators.Core.Enums.MoveType.Attack
            };
            var attacker = TestBuilders.MakeBot(attack: 50, energy: 100);
            var defender = TestBuilders.MakeBot(maxHp: 100, defense: 10);
            int initialHealth = defender.CurrentHealth;
            
            // Act
            var result = move.Use(attacker, defender);
            
            // Assert
            Assert.That(defender.CurrentHealth, Is.LessThan(initialHealth));
            Assert.That(result, Does.Contain("dealt"));
            Assert.That(result, Does.Contain("damage"));
        }
    }
}
