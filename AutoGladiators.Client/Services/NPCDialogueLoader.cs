using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models; // use your canonical DialogueNode/DialogueOption models

namespace AutoGladiators.Client.Services
{
    /// <summary>
    /// Minimal shape your VM expects: an NPC name and a list of DialogueNodes.
    /// ViewModel uses `var dialogue = await ...` so the concrete type can be internal.
    /// </summary>
    public sealed class DialogueData
    {
        public string NPCName { get; init; } = string.Empty;
        public List<DialogueNode> DialogueNodes { get; init; } = new();
    }

    public static class NPCDialogueLoader
    {
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>
        /// Loads a dialogue JSON by filename, e.g. "Assets/Dialogues/lila_trader.json".
        /// Works with MAUI assets or "Copy to Output Directory".
        /// </summary>
        public static async Task<DialogueData> LoadDialogueAsync(string filename, CancellationToken ct = default)
        {
            // 1) Try MAUI asset
            try
            {
#if ANDROID || IOS || MACCATALYST || WINDOWS
                using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
                return await FromStream(stream, ct);
#endif
            }
            catch { /* fall through */ }

            // 2) Try output directory
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, filename);
                if (File.Exists(path))
                {
                    await using var fs = File.OpenRead(path);
                    return await FromStream(fs, ct);
                }
            }
            catch { /* ignore */ }

            // 3) Try app data dir
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, filename);
                if (File.Exists(path))
                {
                    await using var fs = File.OpenRead(path);
                    return await FromStream(fs, ct);
                }
            }
            catch { /* ignore */ }

            // Fallback safe placeholder
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
            var json = await reader.ReadToEndAsync();

            // Try to deserialize flexible shapes:
            var raw = JsonSerializer.Deserialize<RawDialogue>(json, _json);
            if (raw is null)
                return new DialogueData();

            var name = raw.NpcName ?? raw.NPCName ?? "Unknown";
            var nodes = raw.Nodes ?? raw.DialogueNodes ?? new List<DialogueNode>();

            return new DialogueData
            {
                NPCName = name,
                DialogueNodes = nodes
            };
        }

        /// <summary>
        /// Flexible DTO for JSON parsing; maps common field spellings.
        /// </summary>
        private sealed class RawDialogue
        {
            public string? NpcName { get; set; }         // "npcName"
            public string? NPCName { get; set; }         // "NPCName"
            public List<DialogueNode>? Nodes { get; set; }          // "nodes"
            public List<DialogueNode>? DialogueNodes { get; set; }  // "dialogueNodes"
        }

        // Optional helper if you load by NPC id elsewhere
        public static Task<DialogueData> LoadDialogueByNpcIdAsync(string npcId, CancellationToken ct = default)
            => LoadDialogueAsync($"Assets/Dialogues/{npcId}.json", ct);
    }
}
