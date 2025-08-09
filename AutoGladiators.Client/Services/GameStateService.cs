using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using System.Collections.Generic;

namespace AutoGladiators.Client.Services
{
    public partial class GameStateService
    {
        public Action<string, bool>? SetQuestFlag { get; set; }

        // Optional: called in the VM after healing; no-op if you don’t persist bots here
        public void SaveBot(GladiatorBot bot) { /* no-op or persist */ }
        
        public PlayerProfile CurrentPlayer { get; private set; }
        public List<GladiatorBot> BotRoster { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerLocation PlayerLocation { get; set; } = new PlayerLocation();

        public GladiatorBot? CurrentEncounter { get; set; }


        public static GameStateService Instance { get; private set; } = new GameStateService();

        public string CurrentScene => CurrentPlayer?.CurrentLocation;
        public Dictionary<string, bool> StoryFlags => CurrentPlayer?.StoryFlags;

        private GameStateService() { }

        public void InitializeNewGame(string playerName)
        {
            CurrentPlayer = new PlayerProfile
            {
                playerName = playerName,
                Level = 1,
                StoryFlags = new Dictionary<string, bool>() // Initial empty flags
            };
            BotRoster.Clear();
            Inventory.Clear();
            PlayerLocation = new PlayerLocation { Region = "StarterZone", X = 0, Y = 0 };
        }

        public void LoadGame(GameData data)
        {
            CurrentPlayer = data.PlayerProfile;
            BotRoster = new List<GladiatorBot>(data.OwnedBots);
            // Ensure correct type for Inventory.Items
            Inventory.Items = new List<AutoGladiators.Client.Models.Item>(data.Inventory);
            PlayerLocation = data.PlayerLocation;
        }

        public GameData SaveGame()
        {
            return new GameData
            {
                PlayerProfile = CurrentPlayer,
                OwnedBots = new List<GladiatorBot>(BotRoster),
                // Ensure correct type for Inventory
                Inventory = new List<AutoGladiators.Client.Models.Item>(Inventory.Items),
                PlayerLocation = PlayerLocation
            };
        }

        // --- Bot Handling ---
        public GladiatorBot GetCurrentBot()
        {
            return BotRoster.Count > 0 ? BotRoster[0] : null;
        }

        public void AddBotToRoster(GladiatorBot bot)
        {
            BotRoster.Add(bot);
        }

        public void AddBotToRoster(string botId, int level)
        {
            var bot = BotFactory.CreateBot(botId, level);
            if (bot != null)
                BotRoster.Add(bot);
        }

        public void RemoveBotFromRoster(GladiatorBot bot)
        {
            BotRoster.Remove(bot);
        }

        public void UpdateBotInRoster(GladiatorBot bot)
        {
            var index = BotRoster.FindIndex(b => b.Id == bot.Id);
            if (index >= 0)
                BotRoster[index] = bot;
        }

        // --- Inventory Handling ---
        public void AddItem(Item item)
        {
            Inventory.Items.Add(item);
        }

        public void RemoveItem(Item item)
        {
            Inventory.Items.Remove(item);
        }

        public void ClearInventory()
        {
            Inventory.Items.Clear();
        }

        // --- Location & Progress ---
        public void UpdateLocation(PlayerLocation location)
        {
            PlayerLocation = location;
        }

        public void SetPlayerLevel(int level)
        {
            if (CurrentPlayer != null)
                CurrentPlayer.Level = level;
        }

        public void SetPlayerName(string name)
        {
            if (CurrentPlayer != null)
                CurrentPlayer.playerName = name;
        }

        public void SetLastBattleStats(GladiatorBot player, GladiatorBot enemy, bool playerWon)
        {
            // Save stats for review/post-battle
        }

        public void ApplyBattleRewards(int xp, int gold)
        {
            // Add XP and gold to player profile
            // If GainExperience does not exist, replace with direct XP property update:
            // CurrentPlayer.Experience += xp;
            CurrentPlayer.GainExperience(xp);
            CurrentPlayer.Gold += gold;

            // Optionally, update UI or notify listeners
            SetQuestFlag?.Invoke("BattleRewardsApplied", true);
        }
        public void ResetGame()
        {
            CurrentPlayer = new PlayerProfile { playerName = "New Player", Level = 1 };
            BotRoster.Clear();
            Inventory.Clear();
            PlayerLocation = new PlayerLocation { Region = "StarterZone", X = 0, Y = 0 };
        }

        // --- Story & Progression Flags ---
        public bool HasFlag(string key)
        {
            return CurrentPlayer != null && CurrentPlayer.StoryFlags.TryGetValue(key, out var value) && value;
        }

        public void SetFlag(string key, bool value)
        {
            if (CurrentPlayer == null)
                return;

            if (CurrentPlayer.StoryFlags.ContainsKey(key))
                CurrentPlayer.StoryFlags[key] = value;
            else
                CurrentPlayer.StoryFlags.Add(key, value);
        }

        public string GetPlayerLocationId()
        {
            // Return the player's current location ID as string
            return CurrentPlayer?.CurrentLocation?.Region ?? "Unknown";
        }

        public Dictionary<string, bool> QuestFlags { get; set; } = new();

        // --- TODO Hooks ---
        // TODO: Integrate with DialogueService and EncounterService for tracking NPC interactions.
        // TODO: Implement Save/Load to file or persistent storage (e.g., SQLite or JSON).
        // TODO: Add achievement tracking system.
        // TODO: Add support for active quests or tasks.
        // TODO: Hook into event log / message queue system for triggering UI notifications.
        // TODO: Add support for battle history, last seen bots, and usage analytics.

    }
}
