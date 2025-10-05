using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Core.Services
{
    /// <summary>
    /// Manages player profiles, save data, and session persistence
    /// Designed to support future online multiplayer and cloud save integration
    /// </summary>
    public class PlayerProfileService
    {
        private static readonly IAppLogger Log = AppLog.For<PlayerProfileService>();
        
        public static PlayerProfileService Instance { get; } = new();
        
        private PlayerProfile? _currentProfile;
        private readonly string _saveDirectory;
        
        private PlayerProfileService()
        {
            _saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AutoGladiators", 
                "SaveData"
            );
            
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public PlayerProfile? GetCurrentProfile() => _currentProfile;
        public void SetCurrentProfile(PlayerProfile? profile) => _currentProfile = profile;
        public PlayerProfile? CurrentProfile => _currentProfile;
        public bool HasActiveProfile => _currentProfile != null;

        /// <summary>
        /// Creates a new player profile with starter bot selection
        /// </summary>
        public async Task<PlayerProfile> CreateNewProfile(string playerName, string selectedStarterBotId, string nickname)
        {
            return await CreateNewProfile(playerName, "Normal", selectedStarterBotId, nickname);
        }
        
        /// <summary>
        /// Creates a new player profile with starter bot selection and difficulty
        /// </summary>
        public async Task<PlayerProfile> CreateNewProfile(string playerName, string difficulty, string selectedStarterBotId, string nickname)
        {
            try
            {
                Log.Info($"Creating new profile for player: {playerName}");
                
                var profile = new PlayerProfile
                {
                    Id = Guid.NewGuid(),
                    PlayerName = playerName,
                    Difficulty = difficulty,
                    CreatedDate = DateTime.UtcNow,
                    LastPlayedDate = DateTime.UtcNow,
                    PlaytimeMinutes = 0,
                    GameVersion = "1.0.0"
                };

                // Create starter bot with custom nickname
                var starterBot = BotFactory.CreateBot(selectedStarterBotId, 1);
                starterBot.Name = selectedStarterBotId; // Generic name
                starterBot.Nickname = nickname; // Player-assigned nickname
                starterBot.HasOwner = true;
                starterBot.IsStarter = true;
                
                profile.BotRoster.Add(starterBot);
                profile.ActiveBotId = starterBot.Id;

                // Give starting items
                profile.Inventory["Control Chip"] = 5;
                profile.Inventory["Healing Potion"] = 3;
                profile.Inventory["Energy Pack"] = 2;

                // Set starting location
                profile.CurrentMapRegion = "TutorialArea";
                profile.CurrentPosition = new MapPosition { X = 0, Y = 0 };

                _currentProfile = profile;
                await SaveProfile(profile);
                
                Log.Info($"Successfully created profile for {playerName} with starter bot {selectedStarterBotId}");
                return profile;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create new profile: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Loads an existing player profile
        /// </summary>
        public async Task<PlayerProfile?> LoadProfile(Guid profileId)
        {
            try
            {
                string filePath = Path.Combine(_saveDirectory, $"{profileId}.json");
                
                if (!File.Exists(filePath))
                {
                    Log.Warn($"Profile file not found: {profileId}");
                    return null;
                }

                string json = await File.ReadAllTextAsync(filePath);
                var profile = JsonSerializer.Deserialize<PlayerProfile>(json);
                
                if (profile != null)
                {
                    profile.LastPlayedDate = DateTime.UtcNow;
                    _currentProfile = profile;
                    Log.Info($"Successfully loaded profile: {profile.PlayerName}");
                }

                return profile;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load profile {profileId}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets list of all saved profiles
        /// </summary>
        public async Task<List<PlayerProfileSummary>> GetSavedProfiles()
        {
            var summaries = new List<PlayerProfileSummary>();

            try
            {
                var files = Directory.GetFiles(_saveDirectory, "*.json");
                
                foreach (var file in files)
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(file);
                        var profile = JsonSerializer.Deserialize<PlayerProfile>(json);
                        
                        if (profile != null)
                        {
                            summaries.Add(new PlayerProfileSummary
                            {
                                Id = profile.Id,
                                PlayerName = profile.PlayerName,
                                LastPlayedDate = profile.LastPlayedDate,
                                PlaytimeMinutes = profile.PlaytimeMinutes,
                                BotCount = profile.BotRoster.Count,
                                CurrentRegion = profile.CurrentMapRegion
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Failed to read profile file {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to scan save directory: {ex.Message}", ex);
            }

            return summaries;
        }

        /// <summary>
        /// Checks if a profile with the given player name already exists
        /// </summary>
        public async Task<bool> DoesProfileExist(string playerName)
        {
            try
            {
                var profiles = await GetSavedProfiles();
                return profiles.Any(p => string.Equals(p.PlayerName, playerName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check if profile exists for {playerName}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves the current profile
        /// </summary>
        public async Task SaveCurrentProfile()
        {
            if (_currentProfile != null)
            {
                await SaveProfile(_currentProfile);
            }
        }

        /// <summary>
        /// Saves a specific profile
        /// </summary>
        private async Task SaveProfile(PlayerProfile profile)
        {
            try
            {
                string filePath = Path.Combine(_saveDirectory, $"{profile.Id}.json");
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                string json = JsonSerializer.Serialize(profile, options);
                await File.WriteAllTextAsync(filePath, json);
                
                Log.Info($"Profile saved successfully: {profile.PlayerName}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save profile: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Updates playtime for the current profile
        /// </summary>
        public void UpdatePlaytime(int minutesPlayed)
        {
            if (_currentProfile != null)
            {
                _currentProfile.PlaytimeMinutes += minutesPlayed;
                _currentProfile.LastPlayedDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Adds a bot to the current profile's roster
        /// </summary>
        public void AddBotToRoster(GladiatorBot bot)
        {
            if (_currentProfile != null)
            {
                bot.HasOwner = true;
                _currentProfile.BotRoster.Add(bot);
                Log.Info($"Added bot {bot.Name} to roster");
            }
        }

        /// <summary>
        /// Updates the player's current map position
        /// </summary>
        public void UpdateMapPosition(string region, MapPosition position)
        {
            if (_currentProfile != null)
            {
                _currentProfile.CurrentMapRegion = region;
                _currentProfile.CurrentPosition = position;
            }
        }

        /// <summary>
        /// Adds items to the player's inventory
        /// </summary>
        public void AddToInventory(string itemId, int quantity)
        {
            if (_currentProfile != null)
            {
                if (_currentProfile.Inventory.ContainsKey(itemId))
                    _currentProfile.Inventory[itemId] += quantity;
                else
                    _currentProfile.Inventory[itemId] = quantity;
            }
        }

        /// <summary>
        /// Removes items from the player's inventory
        /// </summary>
        public bool ConsumeFromInventory(string itemId, int quantity = 1)
        {
            if (_currentProfile != null && _currentProfile.Inventory.ContainsKey(itemId))
            {
                if (_currentProfile.Inventory[itemId] >= quantity)
                {
                    _currentProfile.Inventory[itemId] -= quantity;
                    if (_currentProfile.Inventory[itemId] <= 0)
                        _currentProfile.Inventory.Remove(itemId);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes a profile (for future cloud save compatibility)
        /// </summary>
        public async Task<bool> DeleteProfile(Guid profileId)
        {
            try
            {
                string filePath = Path.Combine(_saveDirectory, $"{profileId}.json");
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Log.Info($"Profile deleted: {profileId}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete profile {profileId}: {ex.Message}", ex);
                return false;
            }
        }
    }

    /// <summary>
    /// Summary information for profile selection screens
    /// </summary>
    public class PlayerProfileSummary
    {
        public Guid Id { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public DateTime LastPlayedDate { get; set; }
        public int PlaytimeMinutes { get; set; }
        public int BotCount { get; set; }
        public string CurrentRegion { get; set; } = string.Empty;

        public string PlaytimeDisplay => 
            PlaytimeMinutes < 60 
                ? $"{PlaytimeMinutes}m"
                : $"{PlaytimeMinutes / 60}h {PlaytimeMinutes % 60}m";

        public string LastPlayedDisplay =>
            LastPlayedDate.Date == DateTime.Now.Date
                ? "Today"
                : $"{(DateTime.Now - LastPlayedDate).Days} days ago";

        // Compatibility property for existing code
        public DateTime LastPlayed => LastPlayedDate;
    }

    /// <summary>
    /// Represents a position on the exploration map
    /// </summary>
    public class MapPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}