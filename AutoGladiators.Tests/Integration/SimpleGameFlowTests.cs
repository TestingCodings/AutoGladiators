using System;
using System.Threading.Tasks;
using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Tests.Integration
{
    [TestFixture]
    public class SimpleGameFlowTests
    {
        private static readonly IAppLogger Log = AppLog.For<SimpleGameFlowTests>();
        private PlayerProfileService _profileService;
        private GameStateService _gameStateService;

        [SetUp]
        public void SetUp()
        {
            _profileService = new PlayerProfileService();
            _gameStateService = new GameStateService();
            Log.Info("SimpleGameFlowTests setup completed");
        }

        [Test]
        public async Task SimplifiedNewGameFlow_ShouldWork()
        {
            Log.Info("Testing simplified new game flow: Create → Name → Starter → Save");
            
            // Step 1: Create profile with simplified parameters (no difficulty)
            string playerName = "TestTrainer";
            string selectedStarterBotId = "FireStarter";
            string nickname = "Blaze";
            
            var profile = await _profileService.CreateNewProfile(playerName, selectedStarterBotId, nickname);
            
            Assert.IsNotNull(profile, "Profile should be created successfully");
            Assert.AreEqual(playerName, profile.PlayerName, "Player name should match");
            Assert.IsTrue(profile.BotRoster.Count > 0, "Profile should have at least one bot (starter)");
            
            Log.Info($"✓ Profile created: {profile.PlayerName} with {profile.BotRoster.Count} bot(s)");

            // Step 2: Set as current profile (this should sync GameStateService)
            _profileService.SetCurrentProfile(profile);
            
            var currentProfile = _profileService.GetCurrentProfile();
            Assert.IsNotNull(currentProfile, "Current profile should be set");
            Assert.AreEqual(profile.Id, currentProfile.Id, "Current profile should match created profile");
            
            Log.Info("✓ Profile set as current successfully");

            // Step 3: Save the profile
            await _profileService.SaveCurrentProfile();
            
            Log.Info("✓ Profile saved successfully");

            // Step 4: Verify world position is set (required for exploration)
            Assert.IsNotNull(profile.WorldPosition, "Profile should have world position for exploration");
            Log.Info($"✓ World position set: Zone={profile.WorldPosition.ZoneId}, X={profile.WorldPosition.X}, Y={profile.WorldPosition.Y}");

            // Step 5: Test save/load cycle
            var savedProfiles = await _profileService.GetSavedProfiles();
            var savedProfile = savedProfiles.FirstOrDefault(p => p.Id == profile.Id);
            Assert.IsNotNull(savedProfile, "Created profile should be in saved profiles");
            
            Log.Info("✓ Save/load cycle verified");
            
            // Step 6: Verify GameStateService integration
            _gameStateService.LoadProfile(profile);
            Assert.IsNotNull(_gameStateService.CurrentPlayer, "GameStateService should have current player");
            Assert.IsTrue(_gameStateService.BotRoster.Count > 0, "GameStateService should have bot roster");
            
            Log.Info("✓ GameStateService integration verified");

            Log.Info("🎉 Simplified game flow test PASSED");
        }

        [Test]
        public void BotCreation_StarterBots_ShouldWork()
        {
            Log.Info("Testing starter bot creation");
            
            var fireStarter = BotFactory.CreateBot("FireStarter", 1);
            
            Assert.IsNotNull(fireStarter, "Fire starter should be created");
            Assert.AreEqual("Fire", fireStarter.ElementalCore.ToString(), "Fire starter should have Fire element");
            Assert.IsTrue(fireStarter.AttackPower > 0, "Starter should have attack power");
            Assert.IsTrue(fireStarter.MaxHealth > 0, "Starter should have health");
            Assert.IsTrue(fireStarter.Speed > 0, "Starter should have speed");
            
            Log.Info($"✓ Fire starter created: ATK={fireStarter.AttackPower}, HP={fireStarter.MaxHealth}, SPD={fireStarter.Speed}");
        }

        [Test]
        public async Task ProfileManagement_ShouldHandleMultipleProfiles()
        {
            Log.Info("Testing multiple profile management");
            
            // Create first profile
            var profile1 = await _profileService.CreateNewProfile("Trainer1", "FireStarter", "Phoenix");
            await _profileService.SaveProfile(profile1);
            
            // Create second profile  
            var profile2 = await _profileService.CreateNewProfile("Trainer2", "ElectricStarter", "Thunder");
            await _profileService.SaveProfile(profile2);
            
            // Verify both profiles exist
            var savedProfiles = await _profileService.GetSavedProfiles();
            Assert.IsTrue(savedProfiles.Count >= 2, "Should have at least 2 saved profiles");
            
            var trainer1Profile = savedProfiles.FirstOrDefault(p => p.PlayerName == "Trainer1");
            var trainer2Profile = savedProfiles.FirstOrDefault(p => p.PlayerName == "Trainer2");
            
            Assert.IsNotNull(trainer1Profile, "Trainer1 profile should exist");
            Assert.IsNotNull(trainer2Profile, "Trainer2 profile should exist");
            
            Log.Info("✓ Multiple profile management works");
        }
    }
}