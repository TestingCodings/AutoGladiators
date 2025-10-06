using System.Threading.Tasks;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Services;
using AutoGladiators.Tests.Utils;
using NUnit.Framework;

namespace AutoGladiators.Tests.Integration
{
    [TestFixture]
    public class EnemyBotAttackTests
    {
        [Test]
        public void EnemyBot_HasValidMoveset_CanAttackPlayer()
        {
            // Arrange
            var playerBot = TestBuilders.MakeBot("TestPlayer", maxHp: 100, defense: 10);
            var enemyBot = BotFactory.CreateBot("TestEnemy", 3);
            var battleManager = new BattleManager(playerBot, enemyBot);

            var initialPlayerHp = playerBot.CurrentHealth;

            // Act - Enemy chooses and uses a move
            var enemyMove = battleManager.ChooseMove(enemyBot);
            
            // Assert - Enemy should be able to choose a move
            Assert.IsNotNull(enemyMove, "Enemy bot should be able to choose a move");
            Assert.IsNotEmpty(enemyMove.Name, "Enemy move should have a name");

            // Execute the move
            var result = enemyMove.Use(enemyBot, playerBot);

            // Assert - Move should execute (may or may not do damage due to accuracy/stats)
            Assert.IsNotNull(result, "Move execution should return a result");
            Assert.IsNotEmpty(result, "Move result should not be empty");
        }

        [Test]
        public void BattleFlow_EnemyTurn_WorksEndToEnd()
        {
            // Arrange - Set up battle scenario like the real game
            var playerBot = TestBuilders.MakeBot("Player", maxHp: 80, defense: 5);
            var enemyBot = BotFactory.CreateBot("ScrapYardsBot", 2);

            // Verify enemy has moves
            Assert.IsNotNull(enemyBot.Moveset, "Enemy should have moveset");
            Assert.That(enemyBot.Moveset.Count, Is.GreaterThan(0), "Enemy should have moves");

            var battleManager = new BattleManager(playerBot, enemyBot);

            // Act - Simulate enemy turn like BattleViewModel does
            var enemyMove = battleManager.ChooseMove(enemyBot);
            Assert.IsNotNull(enemyMove, "Enemy should choose a move");

            var initialPlayerHp = playerBot.CurrentHealth;
            var result = enemyMove.Use(enemyBot, playerBot);

            // Assert - The battle flow should work
            Assert.IsNotNull(result, "Enemy move should execute");
            
            // The important thing is no exceptions were thrown and the system works
            TestContext.WriteLine($"Enemy used: {enemyMove.Name}");
            TestContext.WriteLine($"Result: {result}");
            TestContext.WriteLine($"Player HP: {initialPlayerHp} -> {playerBot.CurrentHealth}");
        }
    }
}