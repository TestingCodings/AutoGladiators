using System.Linq;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Services;
using AutoGladiators.Tests.Utils;
using NUnit.Framework;

namespace AutoGladiators.Tests.Integration
{
    [TestFixture]
    public class BattleAIIntegrationTests
    {
        [Test]
        public void EnemyBot_CreatedByBotFactory_HasValidMoveset()
        {
            // Arrange - Create an enemy bot like the adventure page does
            var enemyBot = BotFactory.CreateBot("ScrapYardsBot", 3);

            // Assert - Bot should have moves
            Assert.IsNotNull(enemyBot.Moveset, "Enemy bot should have a moveset");
            Assert.That(enemyBot.Moveset.Count, Is.GreaterThan(0), "Enemy bot should have at least one move");
            
            // Assert - All moves should exist in MoveDatabase
            foreach (var moveName in enemyBot.Moveset)
            {
                var move = MoveDatabase.GetMoveByName(moveName);
                Assert.IsNotNull(move, $"Move '{moveName}' should exist in MoveDatabase");
            }
        }

        [Test]
        public void BattleManager_ChooseMove_ReturnsValidMoveForEnemyBot()
        {
            // Arrange
            var playerBot = TestBuilders.MakeBot("Player");
            var enemyBot = BotFactory.CreateBot("TestEnemyBot", 2);
            var battleManager = new BattleManager(playerBot, enemyBot);

            // Act
            var chosenMove = battleManager.ChooseMove(enemyBot);

            // Assert
            Assert.IsNotNull(chosenMove, "BattleManager should choose a valid move for enemy bot");
            Assert.IsNotEmpty(chosenMove.Name, "Chosen move should have a name");
            
            // Verify the move exists in the enemy's moveset
            Assert.That(enemyBot.Moveset.Contains(chosenMove.Name), 
                $"Chosen move '{chosenMove.Name}' should be in enemy's moveset");
        }

        [Test]
        public void BattleFlow_EnemyCanExecuteMoves()
        {
            // Arrange - Set up a battle scenario
            var playerBot = TestBuilders.MakeBot("Player", maxHp: 100);
            var enemyBot = BotFactory.CreateBot("EnemyBot", 2);
            var battleManager = new BattleManager(playerBot, enemyBot);

            var initialPlayerHp = playerBot.CurrentHealth;

            // Act - Have enemy execute a move
            var enemyMove = battleManager.ChooseMove(enemyBot);
            Assert.IsNotNull(enemyMove, "Enemy should be able to choose a move");

            var result = enemyMove.Use(enemyBot, playerBot);

            // Assert - Enemy move should execute successfully
            Assert.IsNotEmpty(result, "Move should return a result message");
            
            // Note: Damage may or may not occur due to accuracy, but move should execute
            // The important thing is that no exceptions were thrown
        }
    }
}