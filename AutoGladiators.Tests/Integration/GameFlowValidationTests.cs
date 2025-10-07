using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Exploration;
using AutoGladiators.Core.Rng;
using System.Linq;

namespace AutoGladiators.Tests.Integration
{
    [TestFixture]
    public class GameFlowValidationTests
    {
        private static readonly IAppLogger Log = AppLog.For<GameFlowValidationTests>();
        private PlayerProfileService _profileService;
        private GameStateService _gameStateService;
        private ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            // Setup service collection like in MauiProgram.cs
            _services = new ServiceCollection();
            
            // Use singleton instances for services that have private constructors
            _services.AddSingleton(PlayerProfileService.Instance);
            _services.AddSingleton(GameStateService.Instance);
            _services.AddSingleton<IRng, DefaultRng>();
            _services.AddSingleton<WorldManager>();
            _services.AddSingleton<MovementManager>();
            _services.AddSingleton<AutoGladiators.Core.Services.Exploration.EncounterService>();
            
            _serviceProvider = _services.BuildServiceProvider();
            _profileService = _serviceProvider.GetRequiredService<PlayerProfileService>();
            _gameStateService = _serviceProvider.GetRequiredService<GameStateService>();
            
            Log.Info("GameFlowValidationTests setup completed");
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProvider?.Dispose();
        }

        [Test]
        public async Task CompleteGameFlow_ShouldWork()
        {
            Log.Info("Testing complete game flow: New Game → Name Character → Select Starter → Save → Enter Exploration");
            
            // Step 1: Create new profile with player name and starter
            string playerName = "TestTrainer";
            string selectedStarterBotId = "FireStarter";
            string nickname = "Blaze";
            
            Log.Info($"Creating profile: Player='{playerName}', Starter='{selectedStarterBotId}', Nickname='{nickname}'");
            
            // Use the simple version without difficulty
            var profile = await _profileService.CreateNewProfile(playerName, selectedStarterBotId, nickname);
            
            Assert.IsNotNull(profile, "Profile should be created successfully");
            Assert.AreEqual(playerName, profile.PlayerName, "Player name should match");
            Assert.IsTrue(profile.BotRoster.Count > 0, "Profile should have at least one bot (starter)");
            
            var starterBot = profile.BotRoster.First();
            Assert.AreEqual(nickname, starterBot.Nickname, "Starter bot should have correct nickname");
            
            Log.Info($"✓ Profile created: {profile.PlayerName} with starter bot '{starterBot.Nickname}'");

            // Step 2: Set as current profile
            _profileService.SetCurrentProfile(profile);
            
            var currentProfile = _profileService.GetCurrentProfile();
            Assert.IsNotNull(currentProfile, "Current profile should be set");
            Assert.AreEqual(profile.Id, currentProfile!.Id, "Current profile should match created profile");
            
            Log.Info("✓ Profile set as current");

            // Step 3: Verify GameStateService integration
            Assert.IsNotNull(_gameStateService.CurrentPlayer, "GameStateService should have current player");
            Assert.AreEqual(profile.Id, _gameStateService.CurrentPlayer!.Id, "GameStateService player should match");
            Assert.IsTrue(_gameStateService.BotRoster.Count > 0, "GameStateService should have bot roster");
            
            Log.Info("✓ GameStateService integration verified");

            // Step 4: Verify world position is set
            Assert.IsNotNull(profile.WorldPosition, "Profile should have world position");
            Log.Info($"✓ World position: Zone={profile.WorldPosition.ZoneId}, X={profile.WorldPosition.X}, Y={profile.WorldPosition.Y}");

            // Step 6: Test save/load cycle
            await _profileService.SaveCurrentProfile();
            var savedProfiles = await _profileService.GetSavedProfiles();
            
            Assert.IsTrue(savedProfiles.Count > 0, "Should have saved profiles");
            var savedProfile = savedProfiles.FirstOrDefault(p => p.Id == profile.Id);
            Assert.IsNotNull(savedProfile, "Created profile should be in saved profiles");
            
            Log.Info("✓ Save/load cycle completed");

            Log.Info("🎉 Complete game flow test PASSED");
        }

        [Test]
        public void StarterBots_ShouldBeAvailable()
        {
            Log.Info("Testing starter bot availability");
            
            // Get available starters from BotFactory using CreateBot method
            var fireStarter = BotFactory.CreateBot("FireStarter", 1);
            var electricStarter = BotFactory.CreateBot("ElectricStarter", 1);
            var iceStarter = BotFactory.CreateBot("IceStarter", 1);
            var metalStarter = BotFactory.CreateBot("MetalStarter", 1);
            var windStarter = BotFactory.CreateBot("WindStarter", 1);
            
            Assert.IsNotNull(fireStarter, "Fire starter should be available");
            Assert.IsNotNull(electricStarter, "Electric starter should be available");
            Assert.IsNotNull(iceStarter, "Ice starter should be available");
            Assert.IsNotNull(metalStarter, "Metal starter should be available");
            Assert.IsNotNull(windStarter, "Wind starter should be available");
            
            Log.Info("✓ All starter bots are available");
            
            // Verify they have proper stats
            Assert.IsTrue(fireStarter.AttackPower > 0, "Fire starter should have attack power");
            Assert.IsTrue(fireStarter.MaxHealth > 0, "Fire starter should have health");
            Assert.IsTrue(fireStarter.Speed > 0, "Fire starter should have speed");
            
            Log.Info($"✓ Starter stats verified - Fire: ATK={fireStarter.AttackPower}, HP={fireStarter.MaxHealth}, SPD={fireStarter.Speed}");
        }

        [Test]
        public async Task ProfileCreation_WithoutDifficulty_ShouldWork()
        {
            Log.Info("Testing simplified profile creation without difficulty selection");
            
            string playerName = "SimpleTrainer";
            string selectedStarterBotId = "FireStarter";
            string nickname = "Phoenix";
            
            // Test the simple version without difficulty
            var profile = await _profileService.CreateNewProfile(playerName, selectedStarterBotId, nickname);
            
            Assert.IsNotNull(profile, "Profile should be created without difficulty");
            Assert.AreEqual(playerName, profile.PlayerName, "Player name should be correct");
            
            var starterBot = profile.BotRoster.FirstOrDefault();
            Assert.IsNotNull(starterBot, "Should have starter bot");
            Assert.AreEqual(nickname, starterBot!.Nickname, "Bot should have correct nickname");
            
            Log.Info("✓ Simplified profile creation works");
        }

        [Test]
        public void ExplorationServices_ShouldBeRegistered()
        {
            Log.Info("Testing exploration service registration");
            
            var worldManager = _serviceProvider.GetService<WorldManager>();
            var movementManager = _serviceProvider.GetService<MovementManager>();
            var encounterService = _serviceProvider.GetService<AutoGladiators.Core.Services.Exploration.EncounterService>();
            var rngService = _serviceProvider.GetService<IRng>();
            
            Assert.IsNotNull(worldManager, "WorldManager should be registered");
            Assert.IsNotNull(movementManager, "MovementManager should be registered");
            Assert.IsNotNull(encounterService, "EncounterService should be registered");
            Assert.IsNotNull(rngService, "RNG service should be registered");
            
            Log.Info("✓ All exploration services are properly registered");
        }
    }
}