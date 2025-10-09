using NUnit.Framework;
using AutoGladiators.Core.Models.Combat;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using CombatMoveDatabase = AutoGladiators.Core.Models.Combat.MoveDatabase;

namespace AutoGladiators.Tests.Combat
{
    [TestFixture]
    public class PokemonMoveDatabaseTests
    {
        [Test]
        public void GetMove_ThunderBolt_ReturnsCorrectMove()
        {
            // Act
            var move = CombatMoveDatabase.GetMove("thunder_bolt");
            
            // Assert
            Assert.That(move, Is.Not.Null);
            Assert.That(move!.Name, Is.EqualTo("Thunder Bolt"));
            Assert.That(move.BasePower, Is.EqualTo(90));
            Assert.That(move.Accuracy, Is.EqualTo(100));
            Assert.That(move.Element, Is.EqualTo(ElementalCore.Electric));
        }
        
        [Test]
        public void GetMove_FlameTest_ReturnsCorrectMove()
        {
            // Act
            var move = CombatMoveDatabase.GetMove("flame_thrower");
            
            // Assert
            Assert.That(move, Is.Not.Null);
            Assert.That(move!.Name, Is.EqualTo("Flame Thrower"));
            Assert.That(move.BasePower, Is.EqualTo(90));
            Assert.That(move.InflictedStatus, Is.EqualTo(StatusEffectType.Burn));
        }
        
        [Test]
        public void GetMove_InvalidId_ReturnsNull()
        {
            // Act
            var move = CombatMoveDatabase.GetMove("nonexistent_move");
            
            // Assert
            Assert.That(move, Is.Null);
        }
        
        [Test]
        public void GetAllMoves_ReturnsAllPokemonStyleMoves()
        {
            // Act
            var allMoves = CombatMoveDatabase.GetAllMoves();
            
            // Assert
            Assert.That(allMoves, Is.Not.Empty);
            Assert.That(allMoves.Any(m => m.Name == "Thunder Bolt"), Is.True);
            Assert.That(allMoves.Any(m => m.Name == "Tackle"), Is.True);
            Assert.That(allMoves.Any(m => m.Name == "Flame Thrower"), Is.True);
        }
        
        [Test]
        public void GetMovesByElement_Electric_ReturnsElectricMoves()
        {
            // Act
            var electricMoves = CombatMoveDatabase.GetMovesByElement(ElementalCore.Electric);
            
            // Assert
            Assert.That(electricMoves, Is.Not.Empty);
            Assert.That(electricMoves.All(m => m.Element == ElementalCore.Electric), Is.True);
        }
        
        [Test]
        public void CalculateDamage_WithSTAB_ReturnsBonusDamage()
        {
            // Arrange
            var move = CombatMoveDatabase.GetMove("thunder_bolt");
            var electricBot = new GladiatorBot 
            { 
                Name = "ElectricBot", 
                ElementalCore = ElementalCore.Electric,
                Level = 20,
                AttackPower = 50,
                CurrentHealth = 100,
                MaxHealth = 100
            };
            var targetBot = new GladiatorBot 
            { 
                Name = "TargetBot", 
                ElementalCore = ElementalCore.Metal,
                Level = 20,
                Defense = 40,
                CurrentHealth = 100,
                MaxHealth = 100
            };
            
            // Act
            var damage = move?.CalculateDamage(electricBot, targetBot) ?? 0;
            
            // Assert
            Assert.That(damage, Is.GreaterThan(0));
        }
    }
}