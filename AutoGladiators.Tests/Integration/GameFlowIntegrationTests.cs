using NUnit.Framework;
using System;
using System.Threading.Tasks;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.StateMachine.States;
using AutoGladiators.Core.Services.Logging;
using System.Collections.Generic;

namespace AutoGladiators.Tests.Integration
{
    /// <summary>
    /// Integration tests for complete game flows to catch initialization and state management issues
    /// </summary>
    [TestFixture]
    public class GameFlowIntegrationTests
    {
        private PlayerProfileService _profileService;
        private GameStateService _gameStateService;
        private static readonly IAppLogger Log = AppLog.For<GameFlowIntegrationTests>();

        [SetUp]
        public void Setup()
        {
            _profileService = PlayerProfileService.Instance;
            _gameStateService = GameStateService.Instance;
            
            // Clear any existing data
            _profileService.SetCurrentProfile(null);
            _gameStateService.BotRoster.Clear();
        }

        [Test]
        public async Task StarterBotSelection_ToWorldExploration_ShouldWork()
        {
            // Arrange
            string playerName = "TestPlayer";
            string difficulty = "Normal";
            string starterBotId = "FireStarter";
            string nickname = "Blazey";

            // Act - Create new profile with starter bot
            var profile = await _profileService.CreateNewProfile(playerName, difficulty, starterBotId, nickname);
            
            // Assert - Profile created successfully
            Assert.IsNotNull(profile, "Profile should be created");
            Assert.AreEqual(playerName, profile.PlayerName);
            Assert.AreEqual(1, profile.BotRoster.Count, "Should have one starter bot");
            
            var starterBot = profile.BotRoster[0];
            Assert.AreEqual(nickname, starterBot.Nickname);
            Assert.IsTrue(starterBot.IsStarter);
            Assert.IsTrue(starterBot.HasOwner);

            // Act - Set as current profile (this should sync GameStateService)
            _profileService.SetCurrentProfile(profile);

            // Assert - GameStateService should be synchronized
            var currentBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(currentBot, "GameStateService should have the current bot");
            Assert.AreEqual(starterBot.Id, currentBot.Id);
            Assert.AreEqual(nickname, currentBot.Nickname);

            Log.Info($"✓ Starter bot selection and sync completed successfully");
        }

        [Test]
        public async Task ContinueGame_LoadProfile_ShouldRestoreState()
        {
            // Arrange - First create and save a profile
            var originalProfile = await _profileService.CreateNewProfile("TestPlayer", "ElectricStarter", "Sparky");
            
            // Clear current state to simulate app restart
            _profileService.SetCurrentProfile(null);
            _gameStateService.BotRoster.Clear();

            // Act - Load the profile
            var loadedProfile = await _profileService.LoadProfile(originalProfile.Id);
            Assert.IsNotNull(loadedProfile, "Profile should load successfully");
            
            // Debug information
            Log.Info($"Loaded profile: PlayerName={loadedProfile.PlayerName}, ActiveBotId={loadedProfile.ActiveBotId}, BotRoster.Count={loadedProfile.BotRoster.Count}");
            if (loadedProfile.BotRoster.Count > 0)
            {
                var firstBot = loadedProfile.BotRoster[0];
                Log.Info($"First bot: Id={firstBot.Id}, Nickname={firstBot.Nickname}");
            }
            
            _profileService.SetCurrentProfile(loadedProfile);
            
            // Debug GameStateService state
            Log.Info($"GameStateService: BotRoster.Count={_gameStateService.BotRoster.Count}, CurrentPlayer.ActiveBotId={_gameStateService.CurrentPlayer?.ActiveBotId}");

            // Assert - State should be restored
            var currentBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(currentBot, "Current bot should be restored");
            Assert.AreEqual("Sparky", currentBot.Nickname);
            Assert.AreEqual(1, _gameStateService.BotRoster.Count);

            Log.Info($"✓ Continue game profile loading completed successfully");
        }

        [Test]
        public async Task BattleSystem_Initialization_ShouldNotThrowNull()
        {
            // Arrange - Set up a complete battle scenario
            var playerProfile = await _profileService.CreateNewProfile("BattleTester", "Normal", "MetalStarter", "Tank");
            _profileService.SetCurrentProfile(playerProfile);

            var playerBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(playerBot, "Player bot should be available");

            var enemyBot = BotFactory.CreateBot("TestEnemyBot", 2);
            Assert.IsNotNull(enemyBot, "Enemy bot should be created");
            
            _gameStateService.CurrentEncounter = enemyBot;

            // Act - Initialize battle state (this should not throw null reference exceptions)
            var battleState = new BattlingState();
            var context = CreateTestGameStateContext();
            var args = new StateArgs { Reason = "TestBattle" };

            // This should complete without throwing dictionary argument null exceptions
            Assert.DoesNotThrowAsync(async () => 
            {
                await battleState.EnterAsync(context, args);
                await battleState.ExecuteAsync(context);
                await battleState.ExitAsync(context);
            }, "Battle state should handle initialization without null reference exceptions");

            Log.Info($"✓ Battle system initialization completed successfully");
        }

        [Test]
        public void BotFactory_CreateStarters_ShouldGenerateValidBots()
        {
            // Arrange - Test all starter bot types
            var starterBotIds = new[] { "FireStarter", "ElectricStarter", "IceStarter", "MetalStarter", "WindStarter" };

            foreach (var botId in starterBotIds)
            {
                // Act
                var bot = BotFactory.CreateBot(botId, 1);

                // Assert
                Assert.IsNotNull(bot, $"Bot {botId} should be created");
                Assert.AreEqual(botId, bot.Name, $"Bot should have correct name");
                Assert.AreEqual(1, bot.Level, $"Bot should be level 1");
                Assert.Greater(bot.MaxHealth, 0, $"Bot should have positive health");
                Assert.Greater(bot.AttackPower, 0, $"Bot should have positive attack");
                Assert.AreEqual(bot.MaxHealth, bot.CurrentHealth, $"Bot should start at full health");

                Log.Info($"✓ {botId} created successfully with stats: HP={bot.MaxHealth}, ATK={bot.AttackPower}, DEF={bot.Defense}");
            }
        }

        [Test]
        public async Task ProfilePersistence_SaveAndLoad_ShouldMaintainIntegrity()
        {
            // Arrange - Create a rich profile with multiple bots and items
            var profile = await _profileService.CreateNewProfile("PersistenceTest", "Hard", "WindStarter", "Gale");
            
            // Add extra data to test full persistence
            profile.Inventory["Control Chip"] = 5;
            profile.Inventory["Healing Potion"] = 3;
            profile.ExplorationPoints = 150;
            profile.SetGameFlag("TutorialComplete", true);

            // Add a second bot to roster
            var secondBot = BotFactory.CreateBot("FireStarter", 3);
            secondBot.Nickname = "Flame";
            secondBot.HasOwner = true;
            profile.BotRoster.Add(secondBot);

            // Act - Set current profile, save it, and reload
            _profileService.SetCurrentProfile(profile);
            
            // Debug: Check profile before saving
            Log.Info($"Before save: PlayerName='{profile.PlayerName}', Difficulty='{profile.Difficulty}'");
            
            await _profileService.SaveCurrentProfile();
            var reloadedProfile = await _profileService.LoadProfile(profile.Id);

            // Debug: Check profile after loading
            Log.Info($"After load: PlayerName='{reloadedProfile?.PlayerName ?? "NULL"}', Difficulty='{reloadedProfile?.Difficulty ?? "NULL"}'");

            // Assert - All data should be preserved
            Assert.IsNotNull(reloadedProfile, "Profile should reload");
            Assert.AreEqual(profile.PlayerName, reloadedProfile.PlayerName);
            Assert.AreEqual(profile.Difficulty, reloadedProfile.Difficulty);
            Assert.AreEqual(2, reloadedProfile.BotRoster.Count, "Should have 2 bots");
            Assert.AreEqual(profile.ExplorationPoints, reloadedProfile.ExplorationPoints);
            Assert.AreEqual(5, reloadedProfile.Inventory.GetValueOrDefault("Control Chip", 0));
            Assert.AreEqual(3, reloadedProfile.Inventory.GetValueOrDefault("Healing Potion", 0));
            Assert.IsTrue(reloadedProfile.GetGameFlag("TutorialComplete"));

            Log.Info($"✓ Profile persistence maintained integrity across save/load cycle");
        }

        [Test]
        public void GameStateService_MultipleProfiles_ShouldSwitchCorrectly()
        {
            // This test ensures GameStateService properly switches between different profiles
            
            // Arrange - Create two different profiles
            var profile1 = new PlayerProfile
            {
                Id = Guid.NewGuid(),
                PlayerName = "Player1",
                Difficulty = "Normal"
            };
            
            var bot1 = BotFactory.CreateBot("FireStarter", 1);
            bot1.Nickname = "Flame1";
            profile1.BotRoster.Add(bot1);

            var profile2 = new PlayerProfile
            {
                Id = Guid.NewGuid(), 
                PlayerName = "Player2",
                Difficulty = "Hard"
            };
            
            var bot2 = BotFactory.CreateBot("ElectricStarter", 1);
            bot2.Nickname = "Spark2";
            profile2.BotRoster.Add(bot2);

            // Act & Assert - Switch between profiles
            _profileService.SetCurrentProfile(profile1);
            var currentBot = _gameStateService.GetCurrentBot();
            Assert.AreEqual("Flame1", currentBot?.Nickname);

            _profileService.SetCurrentProfile(profile2);
            currentBot = _gameStateService.GetCurrentBot();
            Assert.AreEqual("Spark2", currentBot?.Nickname);

            Log.Info($"✓ Profile switching works correctly");
        }

        private GameStateContext CreateTestGameStateContext()
        {
            // Create a minimal context for testing
            return new GameStateContext(_gameStateService, null)
            {
                // Add any required test setup
            };
        }
    }
}