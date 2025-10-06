using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.StateMachine.States; // CapturingResult used by CapturingState
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Enums;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public partial class GameStateService
    {
        private static readonly IAppLogger Log = AppLog.For<GameStateService>();
        private readonly Random _random = new Random();

        // Optional notifier you can hook from a VM
        public Action<string, bool>? SetQuestFlag { get; set; }

        // Singleton
        public static GameStateService Instance { get; private set; } = new GameStateService();
        private GameStateService() { }

        // Core state
        public AutoGladiators.Core.Core.PlayerProfile? CurrentPlayer { get; private set; }
        public List<GladiatorBot> BotRoster { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerLocation PlayerLocation { get; set; } = new PlayerLocation();

        // Encounter tracked by Exploring â†’ Battling/Capturing
        public GladiatorBot? CurrentEncounter { get; set; }

        // Dialogue convenience
        public string? CurrentNpcId { get; private set; }
        public void SetCurrentNpcId(string? npcId)
        {
            CurrentNpcId = npcId;
            SetFlag("CurrentNpcId", npcId ?? string.Empty);
        }

        // Simple key/value â€œsession flagsâ€ used by states (DialogueState, InventoryState, etc.)
        private readonly Dictionary<string, string> _flags = new(StringComparer.OrdinalIgnoreCase);

        // Convenience properties
        public string CurrentScene => CurrentPlayer?.CurrentLocation?.Region ?? "Unknown";

        // DO NOT use ??= with a null-conditional here; it causes CS0131.
        // Initialize story flags safely and return a non-null dictionary.
        private Dictionary<string, bool>? _storyFlagsFallback;
        public Dictionary<string, bool> StoryFlags
        {
            get
            {
                if (CurrentPlayer != null)
                {
                    return CurrentPlayer.StoryFlags;
                }
                return _storyFlagsFallback ??= new Dictionary<string, bool>();
            }
        }

        // ------------------------------
        // Lifecycle / Save-Load
        // ------------------------------
        public void InitializeNewGame(string playerName)
        {
            CurrentPlayer = new PlayerProfile
            {
                PlayerName = playerName
            };
            
            // Add starting gold
            CurrentPlayer.AddItem("Gold", 100);
            
            BotRoster.Clear();
            
            // Add starter bot
            var starterBot = new GladiatorBot
            {
                Id = 1,
                Name = "Starter",
                Level = 1,
                ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal,
                Description = "Your faithful starting companion",
                
                MaxHealth = 100,
                CurrentHealth = 100,
                AttackPower = 20,
                Defense = 15,
                Speed = 12,
                Energy = 100,
                Endurance = 50,
                Luck = 10,
                
                Moveset = new List<string> { "Tackle", "Guard", "Metal Strike" },
                LearnableMoves = new List<string> { "Tackle", "Guard", "Metal Strike", "Power Boost" }
            };
            
            BotRoster.Add(starterBot);
            
            Inventory.Clear();
            PlayerLocation = new PlayerLocation { Region = "StarterZone", X = 0, Y = 0 };
            _flags.Clear();
            CurrentEncounter = null;
            CurrentNpcId = null;
        }

        public void LoadGame(GameData data)
        {
            CurrentPlayer = data.PlayerProfile;
            BotRoster = new List<GladiatorBot>(data.OwnedBots);
            // Explicitly use Models.Item to avoid collisions with any Services.Item
            Inventory.Items = new List<AutoGladiators.Core.Models.Item>(data.Inventory);
            PlayerLocation = data.PlayerLocation;
            _flags.Clear();
            CurrentEncounter = null;
            CurrentNpcId = null;
        }

        public GameData SaveGame()
        {
            return new GameData
            {
                PlayerProfile = CurrentPlayer!,
                OwnedBots = new List<GladiatorBot>(BotRoster),
                // Explicitly use Models.Item to avoid namespace collision
                Inventory = new List<AutoGladiators.Core.Models.Item>(Inventory.Items),
                PlayerLocation = PlayerLocation
            };
        }

        public void ResetGame() => InitializeNewGame("New Player");

        // ------------------------------
        // Bots
        // ------------------------------
        public GladiatorBot? GetCurrentBot()
        {
            // First try to use the active bot from the current profile
            if (CurrentPlayer?.ActiveBotId != null)
            {
                var activeBot = BotRoster.FirstOrDefault(bot => bot.Id == CurrentPlayer.ActiveBotId.Value);
                if (activeBot != null)
                    return activeBot;
            }
            
            // Fallback to first bot in roster
            return BotRoster.Count > 0 ? BotRoster[0] : null;
        }

        public void AddBotToRoster(GladiatorBot bot)
        {
            if (bot != null) BotRoster.Add(bot);
        }

        public void AddBotToRoster(string botId, int level)
        {
            var bot = BotFactory.CreateBot(botId, level);
            if (bot != null) BotRoster.Add(bot);
        }

        public void RemoveBotFromRoster(GladiatorBot bot)
        {
            BotRoster.Remove(bot);
        }

        public void UpdateBotInRoster(GladiatorBot bot)
        {
            var index = BotRoster.FindIndex(b => Equals(b.Id, bot.Id));
            if (index >= 0) BotRoster[index] = bot;
        }

        public void SaveBot(GladiatorBot bot)
        {
            // no-op stub (persist if you want)
        }

        // ------------------------------
        // Inventory
        // ------------------------------
        public void AddItem(AutoGladiators.Core.Models.Item item) => Inventory.Items.Add(item);
        public void RemoveItem(AutoGladiators.Core.Models.Item item) => Inventory.Items.Remove(item);
        public void ClearInventory() => Inventory.Items.Clear();

        public bool CanUseItem(string itemId, Guid playerBotId, bool inBattle, out string? reason)
        {
            reason = null;
            
            // Check if player has the item
            var item = Inventory.Items.FirstOrDefault(i => 
                string.Equals(i.Name, itemId, StringComparison.OrdinalIgnoreCase));
            
            if (item == null)
            {
                reason = $"You don't have any {itemId}.";
                return false;
            }
            
            // Find the target bot
            int targetBotId = unchecked(playerBotId.GetHashCode()); // Convert Guid to int for comparison
            var targetBot = BotRoster.FirstOrDefault(b => b.Id == targetBotId);
            
            if (targetBot == null)
            {
                reason = "Target bot not found in your roster.";
                return false;
            }
            
            // Item-specific usage rules
            string lowerItemId = itemId.ToLowerInvariant();
            
            // Healing items
            if (lowerItemId.Contains("potion") || lowerItemId.Contains("heal"))
            {
                if (targetBot.CurrentHealth >= targetBot.MaxHealth)
                {
                    reason = $"{targetBot.Name} is already at full health.";
                    return false;
                }
            }
            
            // Energy restoration items
            if (lowerItemId.Contains("energy") || lowerItemId.Contains("ether"))
            {
                if (targetBot.Energy >= targetBot.MaxEnergy)
                {
                    reason = $"{targetBot.Name} already has full energy.";
                    return false;
                }
            }
            
            // Capture items - only usable in battle against wild bots
            if (lowerItemId.Contains("capsule") || lowerItemId.Contains("ball"))
            {
                if (!inBattle)
                {
                    reason = "Capture items can only be used during battle.";
                    return false;
                }
                
                if (CurrentEncounter == null)
                {
                    reason = "No wild bot to capture.";
                    return false;
                }
                
                // Can't capture already owned bots
                if (BotRoster.Any(b => b.Id == CurrentEncounter.Id))
                {
                    reason = "You already own this bot.";
                    return false;
                }
            }
            
            // Battle-only items
            if (lowerItemId.Contains("battle") || lowerItemId.Contains("combat"))
            {
                if (!inBattle)
                {
                    reason = "This item can only be used during battle.";
                    return false;
                }
            }
            
            // Evolution items
            if (lowerItemId.Contains("stone") || lowerItemId.Contains("shard") || 
                lowerItemId.Contains("crystal") || lowerItemId.Contains("gem"))
            {
                if (inBattle)
                {
                    reason = "Evolution items cannot be used during battle.";
                    return false;
                }
                
                // Check if bot is at appropriate level for evolution
                if (targetBot.Level < 15)
                {
                    reason = $"{targetBot.Name} must be at least level 15 to evolve.";
                    return false;
                }
            }
            
            // Training items
            if (lowerItemId.Contains("protein") || lowerItemId.Contains("vitamin") ||
                lowerItemId.Contains("supplement"))
            {
                if (inBattle)
                {
                    reason = "Training items cannot be used during battle.";
                    return false;
                }
            }
            
            // Status cure items
            if (lowerItemId.Contains("antidote") || lowerItemId.Contains("cure") ||
                lowerItemId.Contains("remedy"))
            {
                // TODO: Check if bot has status effects when status system is implemented
                // For now, allow usage
            }
            
            return true; // Item can be used
        }

        public void ConsumeItem(string itemId, int count)
        {
            // Match by Name only (your Item doesnâ€™t expose Id)
            for (int i = 0; i < count; i++)
            {
                var idx = Inventory.Items.FindIndex(it =>
                    string.Equals(it.Name, itemId, StringComparison.OrdinalIgnoreCase));

                if (idx >= 0) Inventory.Items.RemoveAt(idx);
                else break;
            }
        }


        // ------------------------------
        // Encounters (Exploring/Battle/Capture)
        // ------------------------------
        public GladiatorBot? GetEncounteredBot() => CurrentEncounter;

        public void MarkEncounterResolved(int botId)
        {
            if (CurrentEncounter != null && Equals(CurrentEncounter.Id, botId))
                CurrentEncounter = null;
        }

        // Capture attempt used by CapturingState
        public Task<CapturingResult?> AttemptCaptureAsync(
            string itemId,
            Guid targetBotId,
            double? forcedChance = null,
            CancellationToken ct = default)
        {
            // Production capture system with dynamic success rates
            var target = CurrentEncounter;
            if (target == null)
            {
                return Task.FromResult<CapturingResult?>(new CapturingResult(
                    false, null, new[] { "No bot to capture!" }, itemId, targetBotId));
            }

            // Base capture rate depends on item type and target bot condition
            double baseRate = GetCaptureRateForItem(itemId);
            
            // Modify capture rate based on target bot's condition
            double healthModifier = CalculateHealthModifier(target);
            double levelModifier = CalculateLevelModifier(target);
            double elementModifier = CalculateElementModifier(target);
            
            // Apply player's capture skill bonus if implemented
            double skillModifier = 1.0; // TODO: Implement capture skill system
            
            // Final capture rate calculation
            double finalRate = forcedChance ?? (baseRate * healthModifier * levelModifier * elementModifier * skillModifier);
            finalRate = Math.Max(0.05, Math.Min(0.95, finalRate)); // Clamp between 5% and 95%
            
            bool success = _random.NextDouble() < finalRate;
            
            GladiatorBot? captured = null;
            List<string> messages = new();
            
            if (success)
            {
                // Create captured bot with slight stat variation
                captured = CreateCapturedBot(target);
                BotRoster.Add(captured);
                CurrentEncounter = null; // Clear encounter
                
                messages.Add($"Used {GetItemDisplayName(itemId)}!");
                messages.Add($"Success! {target.Name} was captured!");
                messages.Add($"{captured.Name} has joined your roster!");
                
                // Consume the capture item
                ConsumeItem(itemId, 1);
                
                // Award experience points for successful capture
                if (CurrentPlayer != null)
                {
                    int expGained = CalculateCaptureExperience(target);
                    CurrentPlayer.ExplorationPoints += expGained;
                    messages.Add($"Gained {expGained} experience points!");
                }
            }
            else
            {
                messages.Add($"Used {GetItemDisplayName(itemId)}!");
                messages.Add($"{target.Name} broke free!");
                
                // Still consume the item on failure
                ConsumeItem(itemId, 1);
                
                // Chance for target to flee or become more aggressive
                if (_random.NextDouble() < 0.3)
                {
                    messages.Add($"{target.Name} fled from battle!");
                    CurrentEncounter = null;
                }
                else
                {
                    messages.Add($"{target.Name} looks more determined to fight!");
                    // Could increase target's stats slightly for next battle
                }
            }
            
            var result = new CapturingResult(success, captured, messages.ToArray(), itemId, targetBotId);
            return Task.FromResult<CapturingResult?>(result);
        }
        
        /// <summary>
        /// Generates a new unique bot ID
        /// </summary>
        private int GetNextBotId()
        {
            // Find the highest existing ID and add 1
            int maxId = 0;
            if (BotRoster.Count > 0)
            {
                maxId = BotRoster.Max(b => b.Id);
            }
            return maxId + 1;
        }
        
        /// <summary>
        /// Gets the base capture rate for different capture items
        /// </summary>
        private double GetCaptureRateForItem(string itemId)
        {
            return itemId.ToLowerInvariant() switch
            {
                "basic capsule" or "capsule" => 0.25,      // 25% base rate
                "great capsule" => 0.40,                   // 40% base rate  
                "ultra capsule" => 0.60,                   // 60% base rate
                "master capsule" => 0.80,                  // 80% base rate
                "net capsule" => 0.35,                     // Better for certain types
                "heavy capsule" => 0.30,                   // Better for high-level bots
                _ => 0.20                                   // Default rate for unknown items
            };
        }
        
        /// <summary>
        /// Calculates capture rate modifier based on target's current health
        /// Lower health = easier to capture
        /// </summary>
        private double CalculateHealthModifier(GladiatorBot target)
        {
            double healthPercent = (double)target.CurrentHealth / target.MaxHealth;
            
            // Easier to capture when health is low
            return healthPercent switch
            {
                <= 0.1 => 3.0,    // Critical health: 3x easier
                <= 0.25 => 2.5,   // Low health: 2.5x easier  
                <= 0.5 => 2.0,    // Half health: 2x easier
                <= 0.75 => 1.5,   // Damaged: 1.5x easier
                _ => 1.0           // Full health: normal rate
            };
        }
        
        /// <summary>
        /// Calculates capture rate modifier based on target's level relative to player's average
        /// Higher level bots are harder to capture
        /// </summary>
        private double CalculateLevelModifier(GladiatorBot target)
        {
            if (BotRoster.Count == 0) return 1.0;
            
            double avgPlayerLevel = BotRoster.Average(b => b.Level);
            double levelDifference = target.Level - avgPlayerLevel;
            
            return levelDifference switch
            {
                <= -10 => 1.8,    // Much lower level: easier to capture
                <= -5 => 1.4,     // Lower level: somewhat easier
                <= 0 => 1.0,      // Equal or slight difference: normal
                <= 5 => 0.7,      // Higher level: harder to capture
                <= 10 => 0.5,     // Much higher level: much harder
                _ => 0.3           // Extremely high level: very difficult
            };
        }
        
        /// <summary>
        /// Calculates capture rate modifier based on elemental affinity
        /// Rare elements are harder to capture
        /// </summary>
        private double CalculateElementModifier(GladiatorBot target)
        {
            return target.ElementalCore switch
            {
                ElementalCore.Plasma => 0.3,     // Plasma is extremely rare
                ElementalCore.Metal => 0.6,      // Metal is uncommon
                ElementalCore.Ice => 0.7,        // Ice is somewhat rare
                ElementalCore.Electric => 0.8,   // Electric is uncommon
                ElementalCore.Wind => 0.8,       // Wind is uncommon
                ElementalCore.Earth => 0.9,      // Earth is common
                ElementalCore.Fire => 1.0,       // Fire is common
                ElementalCore.Water => 1.0,      // Water is common  
                ElementalCore.Grass => 1.1,      // Grass is very common
                _ => 1.0                          // None/unknown: normal
            };
        }
        
        /// <summary>
        /// Creates a captured version of the target bot with slight variations
        /// </summary>
        private GladiatorBot CreateCapturedBot(GladiatorBot original)
        {
            // Create a copy with some variation to make each capture unique
            var captured = new GladiatorBot
            {
                Id = GetNextBotId(),
                Name = original.Name,
                ElementalCore = original.ElementalCore,
                Level = original.Level,
                
                // Add slight stat variation (±10%) to make captures feel unique
                AttackPower = VariateStat(original.AttackPower, 0.1),
                Defense = VariateStat(original.Defense, 0.1),
                Speed = VariateStat(original.Speed, 0.1),
                MaxHealth = VariateStat(original.MaxHealth, 0.1),
                MaxEnergy = VariateStat(original.MaxEnergy, 0.1),
                
                // Copy other properties
                CriticalHitChance = original.CriticalHitChance,
                Experience = (int)(original.Experience * 0.8) // Slightly reduced XP
                // TODO: Copy moves when Move system is implemented
            };
            
            // Set health and energy to full for newly captured bot
            captured.CurrentHealth = captured.MaxHealth;
            captured.Energy = captured.MaxEnergy;
            
            return captured;
        }
        
        /// <summary>
        /// Adds random variation to a stat value
        /// </summary>
        private int VariateStat(int baseStat, double variationPercent)
        {
            double variation = 1.0 + ((_random.NextDouble() - 0.5) * 2 * variationPercent);
            return Math.Max(1, (int)(baseStat * variation));
        }
        
        /// <summary>
        /// Calculates experience points gained from successful capture
        /// </summary>
        private int CalculateCaptureExperience(GladiatorBot target)
        {
            int baseExp = target.Level * 15; // Base experience
            
            // Bonus for rare elements
            double rarityMultiplier = target.ElementalCore switch
            {
                ElementalCore.Plasma => 2.0,
                ElementalCore.Metal => 1.5,
                ElementalCore.Ice => 1.3,
                ElementalCore.Electric => 1.2,
                ElementalCore.Wind => 1.2,
                _ => 1.0
            };
            
            return (int)(baseExp * rarityMultiplier);
        }
        
        /// <summary>
        /// Gets display name for capture items
        /// </summary>
        private string GetItemDisplayName(string itemId)
        {
            return itemId switch
            {
                "basic capsule" => "Basic Capsule",
                "great capsule" => "Great Capsule", 
                "ultra capsule" => "Ultra Capsule",
                "master capsule" => "Master Capsule",
                "net capsule" => "Net Capsule",
                "heavy capsule" => "Heavy Capsule",
                _ => itemId // Return as-is for unknown items
            };
        }

        // ------------------------------
        // Location & Player
        // ------------------------------
        public void UpdateLocation(PlayerLocation location) => PlayerLocation = location;

        public void SetPlayerLevel(int level)
        {
            // Level is computed from ExplorationPoints, so we adjust ExplorationPoints instead
            if (CurrentPlayer != null)
            {
                int targetExperience = (level - 1) * 100;
                CurrentPlayer.ExplorationPoints = Math.Max(0, targetExperience);
            }
        }

        public void SetPlayerName(string name)
        {
            // PlayerName is set at creation and should not be changed during gameplay
            // If name change is needed, create a new profile
            if (CurrentPlayer != null)
            {
                // Log warning about attempting to change immutable player name
                Log.Warn($"Attempted to change player name from {CurrentPlayer.PlayerName} to {name}. Player name is immutable.");
            }
        }

        public string GetPlayerLocationId()
        {
            return CurrentPlayer?.CurrentLocation?.Region ?? "Unknown";
        }

        // ------------------------------
        // Battle results / rewards
        // ------------------------------
        public void SetLastBattleStats(GladiatorBot player, GladiatorBot enemy, bool playerWon)
        {
            // Stub: persist if you want
        }

        public void ApplyBattleRewards(int xp, int gold)
        {
            if (CurrentPlayer != null)
            {
                CurrentPlayer.ExplorationPoints += xp;
                CurrentPlayer.AddItem("Gold", gold);
            }
            SetQuestFlag?.Invoke("BattleRewardsApplied", true);
        }

        // ------------------------------
        // Story flags (bool) and session flags (string)
        // ------------------------------
        public bool HasFlag(string key)
        {
            return CurrentPlayer?.GetGameFlag(key) ?? false;
        }

        public void SetFlag(string key, bool value)
        {
            if (CurrentPlayer == null) return;
            CurrentPlayer.SetGameFlag(key, value);
        }

        // Session flags (string) â€” used by revised states (DialogueState, InventoryState, etc.)
        public string? GetFlag(string key)
        {
            return _flags.TryGetValue(key, out var v) ? v : null;
        }

        public void SetFlag(string key, string value)
        {
            _flags[key] = value;
        }

        // Optional separate quest flag bucket (already present)
        public Dictionary<string, bool> QuestFlags { get; set; } = new();
    }
}

