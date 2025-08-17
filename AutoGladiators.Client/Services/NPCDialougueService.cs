using System.Text.Json;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services
{
    public static class NPCDialogueService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For("NPCDialogueService");
        private static string BasePath => Path.Combine(FileSystem.Current.AppDataDirectory, "Dialogues");

        public static async Task<NPCDialogue> LoadDialogueAsync(string npcId)
        {
            string fileName = $"{npcId}.json";
            string fullPath = Path.Combine(BasePath, fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Dialogue file not found: {fullPath}");

            var json = await File.ReadAllTextAsync(fullPath);
            return JsonSerializer.Deserialize<NPCDialogue>(json);
        }

        public static async Task<List<string>> GetAvailableDialoguesAsync()
        {
            if (!Directory.Exists(BasePath)) return new List<string>();

            var files = Directory.GetFiles(BasePath, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
    }
}

