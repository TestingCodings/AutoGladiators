using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoGladiators.Core.Models;              // DialogueNode / DialogueOption
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    /// <summary>
    /// Minimal shape your VM expects: an NPC name and a list of DialogueNodes.
    /// </summary>
    public sealed class DialogueData
    {
        public string NPCName { get; init; } = string.Empty;
        public List<DialogueNode> DialogueNodes { get; init; } = new();
    }

    public class NPCDialogueLoader
    {
        private readonly IAppStorage _storage;
        private static readonly ILogger Log = (ILogger)AppLog.For<DialogueData>();

        public NPCDialogueLoader(IAppStorage storage)
        {
            _storage = storage;
        }

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>
        /// Loads a dialogue JSON by relative filename, e.g. "Assets/Dialogues/lila_trader.json".
        /// Search order: output directory → app data directory. (No MAUI APIs in Core.)
        /// </summary>
        public async Task<DialogueData> LoadDialogueAsync(string filename, CancellationToken ct = default)
        {
            // 1) Try output directory (AppContext.BaseDirectory)
            var probe1 = Path.Combine(AppContext.BaseDirectory, filename);
            if (File.Exists(probe1))
            {
                await using var fs = File.OpenRead(probe1);
                return await FromStream(fs, ct).ConfigureAwait(false);
            }

            // 2) Try app data dir via IAppStorage
            var probe2 = Path.Combine(_storage.AppDataPath, filename);
            if (File.Exists(probe2))
            {
                await using var fs = File.OpenRead(probe2);
                return await FromStream(fs, ct).ConfigureAwait(false);
            }

            // Fallback placeholder
            return new DialogueData
            {
                NPCName = "Unknown NPC",
                DialogueNodes = new()
                {
                    new DialogueNode
                    {
                        Id = "start",
                        Text = $"Dialogue file not found: {filename}",
                        Options = new() { new DialogueOption { Text = "Close", NextNodeId = "start" } }
                    }
                }
            };
        }

        // Accepts multiple possible JSON shapes and maps them into DialogueData
        private static async Task<DialogueData> FromStream(Stream stream, CancellationToken ct)
        {
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync().ConfigureAwait(false);

            var raw = JsonSerializer.Deserialize<RawDialogue>(json, _json);
            if (raw is null) return new DialogueData();

            var name = raw.NpcName ?? raw.NPCName ?? "Unknown";
            var nodes = raw.Nodes ?? raw.DialogueNodes ?? new List<DialogueNode>();

            return new DialogueData
            {
                NPCName = name,
                DialogueNodes = nodes
            };
        }

        /// <summary> Flexible DTO for JSON parsing; maps common field spellings. </summary>
        private sealed class RawDialogue
        {
            public string? NpcName { get; set; }                    // "npcName"
            public string? NPCName { get; set; }                    // "NPCName"
            public List<DialogueNode>? Nodes { get; set; }          // "nodes"
            public List<DialogueNode>? DialogueNodes { get; set; }  // "dialogueNodes"
        }

        public Task<DialogueData> LoadDialogueByNpcIdAsync(string npcId, CancellationToken ct = default)
            => LoadDialogueAsync($"Assets/Dialogues/{npcId}.json", ct);
    }
}
