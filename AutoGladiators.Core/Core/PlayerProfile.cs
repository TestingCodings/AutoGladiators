using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;

namespace AutoGladiators.Core.Core
{
    public class PlayerProfile
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PlayerName { get; set; } = string.Empty;
        
        [Ignore]
        public List<GladiatorBot> BotRoster { get; set; } = new();
        
        [Ignore]
        public Dictionary<string, int> Inventory { get; set; } = new(); // ItemId -> Quantity
        
        // Game Progress
        public int? ActiveBotId { get; set; }
        public string CurrentMapRegion { get; set; } = "TutorialArea";
        
        [Ignore]
        public MapPosition CurrentPosition { get; set; } = new();
        
        // Exploration System (Pokemon-style)
        [Ignore]
        public WorldPosition WorldPosition { get; set; } = new WorldPosition("starter_town", 10, 10);
        
        [Ignore]
        public HashSet<string> UnlockedRegions { get; set; } = new() { "TutorialArea" };
        
        [Ignore]
        public HashSet<string> CompletedQuests { get; set; } = new();
        
        [Ignore]
        public Dictionary<string, bool> GameFlags { get; set; } = new();
        
        // Session Data
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastPlayedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastPlayed => LastPlayedDate; // Alias for save system compatibility
        public int PlaytimeMinutes { get; set; }
        public string GameVersion { get; set; } = "1.0.0";
        
        // Game Settings (for new save system)
        public string Difficulty { get; set; } = "Normal";
        public bool IsNewProfile { get; set; } = true;
        
        // Statistics
        public int BattlesWon { get; set; }
        public int BattlesLost { get; set; }
        public int BotsCollected { get; set; }
        public int ItemsUsed { get; set; }
        public int ExplorationPoints { get; set; }

        // Legacy compatibility properties
        [Ignore]
        public string playerName => PlayerName;
        
        [Ignore]
        public int Level => PlayerLevel;
        
        [Ignore]
        public int Experience => ExplorationPoints;
        
        [Ignore]
        public int Gold => GetItemQuantity("Gold");
        
        [Ignore]
        public PlayerLocation CurrentLocation => new PlayerLocation { X = CurrentPosition.X, Y = CurrentPosition.Y };
        
        [Ignore]
        public Dictionary<string, bool> StoryFlags => GameFlags;

        // Statistics wrapper for new system compatibility
        [Ignore]
        public PlayerStatistics Statistics => new PlayerStatistics
        {
            Level = PlayerLevel,
            Experience = ExplorationPoints,
            BattlesWon = BattlesWon,
            BattlesLost = BattlesLost,
            TotalPlaytimeMinutes = PlaytimeMinutes
        };
        
        // Game settings wrapper
        [Ignore]
        public GameSettings GameSettings => new GameSettings
        {
            Difficulty = Difficulty
        };

        // Computed Properties
        [Ignore]
        public GladiatorBot? ActiveBot => ActiveBotId.HasValue 
            ? BotRoster.FirstOrDefault(b => b.Id == ActiveBotId.Value)
            : BotRoster.FirstOrDefault();
        
        [Ignore]    
        public int TotalBots => BotRoster.Count;
        
        [Ignore]
        public int PlayerLevel => Math.Max(1, ExplorationPoints / 100 + BotRoster.Count / 3);
        
        [Ignore]
        public double WinRate => BattlesWon + BattlesLost > 0 
            ? (double)BattlesWon / (BattlesWon + BattlesLost) 
            : 0.0;

        // Inventory Management
        public void AddItem(string itemId, int quantity = 1)
        {
            if (Inventory.ContainsKey(itemId))
                Inventory[itemId] += quantity;
            else
                Inventory[itemId] = quantity;
        }

        public bool ConsumeItem(string itemId)
        {
            if (Inventory.ContainsKey(itemId) && Inventory[itemId] > 0)
            {
                Inventory[itemId]--;
                if (Inventory[itemId] <= 0)
                    Inventory.Remove(itemId);
                ItemsUsed++;
                return true;
            }
            return false;
        }

        public void DiscardItem(string itemId)
        {
            if (Inventory.ContainsKey(itemId))
                Inventory.Remove(itemId);
        }

        public int GetItemQuantity(string itemId)
        {
            return Inventory.TryGetValue(itemId, out var count) ? count : 0;
        }

        // Bot Management
        public void AddBot(GladiatorBot bot)
        {
            bot.HasOwner = true;
            BotRoster.Add(bot);
            BotsCollected++;
            
            // Set as active if no active bot
            if (ActiveBotId == null || !BotRoster.Any(b => b.Id == ActiveBotId))
                ActiveBotId = bot.Id;
        }

        public void RemoveBot(int botId)
        {
            var bot = BotRoster.FirstOrDefault(b => b.Id == botId);
            if (bot != null)
            {
                BotRoster.Remove(bot);
                
                // Update active bot if removed
                if (ActiveBotId == botId)
                    ActiveBotId = BotRoster.FirstOrDefault()?.Id;
            }
        }

        public void SetActiveBot(int botId)
        {
            if (BotRoster.Any(b => b.Id == botId))
                ActiveBotId = botId;
        }

        // Progress Tracking
        public void RecordBattleResult(bool won)
        {
            if (won) BattlesWon++; else BattlesLost++;
            LastPlayedDate = DateTime.UtcNow;
        }

        public void UnlockRegion(string regionId)
        {
            UnlockedRegions.Add(regionId);
        }

        public void CompleteQuest(string questId)
        {
            CompletedQuests.Add(questId);
            ExplorationPoints += 50; // Reward for quest completion
            LastPlayedDate = DateTime.UtcNow;
        }

        public void SetGameFlag(string flag, bool value)
        {
            GameFlags[flag] = value;
        }

        public bool GetGameFlag(string flag)
        {
            return GameFlags.TryGetValue(flag, out var value) && value;
        }

        // Update last played timestamp
        public void UpdateLastPlayed()
        {
            LastPlayedDate = DateTime.UtcNow;
        }
    }

    // Helper classes for new save system compatibility
    public class PlayerStatistics
    {
        public int Level { get; set; }
        public int Experience { get; set; }
        public int BattlesWon { get; set; }
        public int BattlesLost { get; set; }
        public int TotalPlaytimeMinutes { get; set; }
    }

    public class GameSettings
    {
        public string Difficulty { get; set; } = "Normal";
    }
}