using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Services;
using AutoGladiators.Tests.Utils;
using NUnit.Framework;

namespace AutoGladiators.Tests.Integration
{
    /// <summary>
    /// Test to verify the complete battle flow works from enemy creation to combat
    /// </summary>
    [TestFixture]
    public class CompleteBattleFlowTests
    {
        [Test]
        public void AdventurePage_EnemyBot_CanFightBack()
        {
            // Arrange - Simulate how AdventurePage creates enemy bots
            var enemyLevel = 2;
            var locationId = "ScrapYards";
            var enemyBot = BotFactory.CreateBot($"{locationId}Bot", enemyLevel);
            
            // Simulate player bot from GameStateService
            var playerBot = TestBuilders.MakeBot("TestPlayer", level: 3, maxHp: 100);
            
            // Create battle manager like BattleViewModel does
            var battleManager = new BattleManager(playerBot, enemyBot);
            
            // Act - Test the exact flow that BattleViewModel.ExecuteEnemyTurn uses
            var enemyMove = battleManager.ChooseMove(enemyBot);
            
            // Assert - This is the critical fix - enemy should now be able to choose moves
            Assert.IsNotNull(enemyBot.Moveset, "Enemy bot should have a moveset");
            Assert.That(enemyBot.Moveset.Count, Is.GreaterThan(0), "Enemy bot should have moves");
            Assert.IsNotNull(enemyMove, "BattleManager.ChooseMove should return a valid move");
            
            // Test move execution
            var initialPlayerHp = playerBot.CurrentHealth;
            var result = enemyMove.Use(enemyBot, playerBot);
            
            Assert.IsNotNull(result, "Enemy move should execute successfully");
            TestContext.WriteLine($"✓ Enemy bot successfully used move: {enemyMove.Name}");
            TestContext.WriteLine($"✓ Move result: {result}");
            TestContext.WriteLine($"✓ Enemy bot has moveset: [{string.Join(", ", enemyBot.Moveset)}]");
        }
        
        [Test]
        public void AllElementTypes_CreateValidBots()
        {
            // Test that all element types can create valid bots with movesets
            var elementTypes = new[] { "Fire", "Water", "Electric", "Ice", "Metal", "Wind", "Earth", "Plasma" };
            
            foreach (var element in elementTypes)
            {
                var bot = BotFactory.CreateBot($"{element}TestBot", 1);
                
                Assert.IsNotNull(bot.Moveset, $"{element} bot should have moveset");
                Assert.That(bot.Moveset.Count, Is.GreaterThan(0), $"{element} bot should have moves");
                
                // Test that all moves exist in MoveDatabase
                foreach (var moveName in bot.Moveset)
                {
                    var move = MoveDatabase.GetMoveByName(moveName);
                    Assert.IsNotNull(move, $"Move '{moveName}' should exist in MoveDatabase for {element} bot");
                }
            }
        }
    }
}