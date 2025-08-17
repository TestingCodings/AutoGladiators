using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine.States; // CapturingResult used by CapturingState
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services
{
    public partial class GameStateService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<GameStateService>();

        // Optional notifier you can hook from a VM
        public Action<string, bool>? SetQuestFlag { get; set; }

        // Singleton
        public static GameStateService Instance { get; private set; } = new GameStateService();
        private GameStateService() { }

        // Core state
        public PlayerProfile? CurrentPlayer { get; private set; }
        public List<GladiatorBot> BotRoster { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerLocation PlayerLocation { get; set; } = new PlayerLocation();

        // Encounter tracked by Exploring → Battling/Capturing
        public GladiatorBot? CurrentEncounter { get; set; }

        // Dialogue convenience
        public string? CurrentNpcId { get; private set; }
        public void SetCurrentNpcId(string? npcId)
        {
            CurrentNpcId = npcId;
            SetFlag("CurrentNpcId", npcId ?? string.Empty);
        }

        // Simple key/value “session flags” used by states (DialogueState, InventoryState, etc.)
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
                    CurrentPlayer.StoryFlags ??= new Dictionary<string, bool>();
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
                playerName = playerName,
                Level = 1,
                Experience = 0,
                Gold = 0,
                StoryFlags = new Dictionary<string, bool>()
            };
            BotRoster.Clear();
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
            Inventory.Items = new List<AutoGladiators.Client.Models.Item>(data.Inventory);
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
                Inventory = new List<AutoGladiators.Client.Models.Item>(Inventory.Items),
                PlayerLocation = PlayerLocation
            };
        }

        public void ResetGame() => InitializeNewGame("New Player");

        // ------------------------------
        // Bots
        // ------------------------------
        public GladiatorBot? GetCurrentBot()
        {
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
        public void AddItem(AutoGladiators.Client.Models.Item item) => Inventory.Items.Add(item);
        public void RemoveItem(AutoGladiators.Client.Models.Item item) => Inventory.Items.Remove(item);
        public void ClearInventory() => Inventory.Items.Clear();

        public bool CanUseItem(string itemId, Guid playerBotId, bool inBattle, out string? reason)
        {
            // Super-permissive stub; tighten rules later
            reason = null;
            return true;
        }

        public void ConsumeItem(string itemId, int count)
        {
            // Match by Name only (your Item doesn’t expose Id)
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
            // Extremely simple stub: 50% base, or use forcedChance if provided
            var chance = forcedChance ?? 0.5;
            var success = new Random().NextDouble() < chance;

            GladiatorBot? captured = null;

            // If your GladiatorBot.Id is an int, we derive a stable-ish int from the Guid.
            int targetBotIntId = unchecked(targetBotId.GetHashCode());

            var target = BotRoster.Find(b => Equals(b.Id, targetBotIntId)) ?? CurrentEncounter;
            if (success)
            {
                captured = target ?? new GladiatorBot
                {
                    Id = targetBotIntId,
                    Name = "Captured Bot",
                    Level = 1
                };
            }

            // Use positional args to avoid named-arg mismatch (CS1739) if record param names differ
            var res = new CapturingResult(
                success,
                captured,
                success ? new[] { $"Used {itemId}. Capture succeeded." }
                        : new[] { $"Used {itemId}. Capture failed." },
                itemId,
                targetBotId
            );

            return Task.FromResult<CapturingResult?>(res);
        }

        // ------------------------------
        // Location & Player
        // ------------------------------
        public void UpdateLocation(PlayerLocation location) => PlayerLocation = location;

        public void SetPlayerLevel(int level)
        {
            if (CurrentPlayer != null) CurrentPlayer.Level = level;
        }

        public void SetPlayerName(string name)
        {
            if (CurrentPlayer != null) CurrentPlayer.playerName = name;
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
                CurrentPlayer.Experience += xp;
                CurrentPlayer.Gold += gold;
            }
            SetQuestFlag?.Invoke("BattleRewardsApplied", true);
        }

        // ------------------------------
        // Story flags (bool) and session flags (string)
        // ------------------------------
        public bool HasFlag(string key)
        {
            return CurrentPlayer != null
                   && (CurrentPlayer.StoryFlags ??= new Dictionary<string, bool>())
                          .TryGetValue(key, out var value)
                   && value;
        }

        public void SetFlag(string key, bool value)
        {
            if (CurrentPlayer == null) return;
            CurrentPlayer.StoryFlags ??= new Dictionary<string, bool>();
            CurrentPlayer.StoryFlags[key] = value;
        }

        // Session flags (string) — used by revised states (DialogueState, InventoryState, etc.)
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

