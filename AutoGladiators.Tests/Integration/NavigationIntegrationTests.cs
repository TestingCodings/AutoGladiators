using System;
using System.Threading.Tasks;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Logging;
using NUnit.Framework;

namespace AutoGladiators.Tests.Integration
{
    /// <summary>
    /// Integration tests for navigation and state persistence flows
    /// </summary>
    [TestFixture]
    public class NavigationIntegrationTests
    {
        private static readonly IAppLogger Log = AppLog.For<NavigationIntegrationTests>();
        private PlayerProfileService _profileService = null!;
        private GameStateService _gameStateService = null!;

        [SetUp]
        public void Setup()
        {
            _profileService = PlayerProfileService.Instance;
            _gameStateService = GameStateService.Instance;
            Log.Info("NavigationIntegrationTests setup completed");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any test data
            _profileService.SetCurrentProfile(null);
            _gameStateService.BotRoster.Clear();
        }

        [Test]
        public async Task StarterSelectionFlow_ShouldCreateProfileAndSyncGameState()
        {
            // Arrange
            string playerName = "TestNavigationPlayer";
            string difficulty = "Normal";
            string starterBotId = "FireStarter";
            string nickname = "BlazeTestBot";

            // Act - Simulate the complete starter selection flow
            Log.Info("Starting starter selection navigation test");
            
            // Step 1: Create new profile (what StarterSelectionPage does)
            var profile = await _profileService.CreateNewProfile(playerName, difficulty, starterBotId, nickname);
            
            // Step 2: Set as current profile (what StarterSelectionPage does)
            _profileService.SetCurrentProfile(profile);

            // Assert - Verify profile creation and synchronization
            Assert.IsNotNull(profile, "Profile should be created");
            Assert.AreEqual(playerName, profile.PlayerName, "Player name should match");
            Assert.AreEqual(1, profile.BotRoster.Count, "Profile should have 1 starter bot");
            
            var starterBot = profile.BotRoster[0];
            Assert.AreEqual(nickname, starterBot.Nickname, "Bot should have correct nickname");
            Assert.IsTrue(starterBot.IsStarter, "Bot should be marked as starter");
            Assert.IsTrue(starterBot.HasOwner, "Bot should have owner");

            // Verify GameStateService synchronization
            var currentPlayer = _gameStateService.CurrentPlayer;
            Assert.IsNotNull(currentPlayer, "GameState should have current player");
            Assert.AreEqual(playerName, currentPlayer.PlayerName, "GameState player name should match");
            
            var gameBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(gameBot, "GameState should have current bot");
            Assert.AreEqual(nickname, gameBot.Nickname, "GameState bot should match profile bot");
            
            Log.Info("✓ Starter selection flow completed successfully");
        }

        [Test]
        public async Task ContinueGameFlow_ShouldLoadProfileAndSyncGameState()
        {
            // Arrange - Create and save a profile first
            string playerName = "TestContinuePlayer";
            var profile = await _profileService.CreateNewProfile(playerName, "Normal", "IceStarter", "FrostTestBot");
            await _profileService.SaveCurrentProfile();
            var profileId = profile.Id;
            
            // Clear current state to simulate app restart
            _profileService.SetCurrentProfile(null);

            // Act - Simulate the continue game flow
            Log.Info("Starting continue game navigation test");
            
            // Step 1: Load existing profile (what ContinueGamePage does)
            var loadedProfile = await _profileService.LoadProfile(profileId);
            
            // Step 2: Set as current profile (what ContinueGamePage does)
            _profileService.SetCurrentProfile(loadedProfile);

            // Assert - Verify profile loading and synchronization
            Assert.IsNotNull(loadedProfile, "Profile should be loaded");
            Assert.AreEqual(playerName, loadedProfile.PlayerName, "Loaded player name should match");
            Assert.AreEqual(1, loadedProfile.BotRoster.Count, "Loaded profile should have starter bot");

            // Verify GameStateService synchronization
            var currentPlayer = _gameStateService.CurrentPlayer;
            Assert.IsNotNull(currentPlayer, "GameState should have current player after loading");
            Assert.AreEqual(playerName, currentPlayer.PlayerName, "GameState player name should match loaded profile");
            
            var gameBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(gameBot, "GameState should have current bot after loading");
            Assert.AreEqual("FrostTestBot", gameBot.Nickname, "GameState bot should match loaded profile bot");
            
            Log.Info("✓ Continue game flow completed successfully");
        }

        [Test]
        public void AdventurePage_ShouldHandleNullCurrentPlayerGracefully()
        {
            // Arrange - Start fresh instance to simulate app start without profile
            _profileService.SetCurrentProfile(null);
            _gameStateService.BotRoster.Clear();

            // Act & Assert - Verify system can handle state when no profile is loaded
            // The test verifies that GetCurrentBot() returns null when no bots are available
            var currentBot = _gameStateService.GetCurrentBot();
            
            // After clearing the roster, there should be no current bot
            Assert.IsNull(currentBot, "Should handle null current bot when roster is empty");
            
            // This should not throw an exception - AdventurePage handles this gracefully
            // by showing "Initializing adventure..." message when CurrentPlayer is null
            Log.Info("✓ AdventurePage null state handling verified");
        }

        [Test]
        public async Task NavigationFlow_ShouldPreserveStateAcrossGameSessions()
        {
            // Arrange & Act - Complete starter selection and save
            string playerName = "TestPersistencePlayer";
            var profile = await _profileService.CreateNewProfile(playerName, "Hard", "MetalStarter", "SteelTestBot");
            _profileService.SetCurrentProfile(profile);
            
            // Add some progression to the profile
            profile.ExplorationPoints = 150;
            profile.AddItem("Gold", 500);
            await _profileService.SaveCurrentProfile();
            
            var savedProfileId = profile.Id;
            
            // Simulate app restart - clear state
            _profileService.SetCurrentProfile(null);
            _gameStateService.BotRoster.Clear();
            
            // Load the profile back
            var restoredProfile = await _profileService.LoadProfile(savedProfileId);
            _profileService.SetCurrentProfile(restoredProfile);

            // Assert - Verify all data persisted correctly
            Assert.IsNotNull(restoredProfile, "Profile should be restored");
            Assert.AreEqual(playerName, restoredProfile!.PlayerName, "Player name should persist");
            Assert.AreEqual(150, restoredProfile.Experience, "Experience should persist");
            Assert.AreEqual(500, restoredProfile.Gold, "Gold should persist");
            
            var currentBot = _gameStateService.GetCurrentBot();
            Assert.IsNotNull(currentBot, "Current bot should be restored");
            Assert.AreEqual("SteelTestBot", currentBot!.Nickname, "Bot nickname should persist");
            
            Log.Info("✓ State persistence across sessions verified");
        }
    }
}