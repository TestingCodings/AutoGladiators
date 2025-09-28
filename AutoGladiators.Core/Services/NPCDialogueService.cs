using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public class NPCDialogueService
    {
        private readonly IAppStorage _storage;
        private static readonly ILogger Log = (ILogger)AppLog.For("NPCDialogueService");

        public NPCDialogueService(IAppStorage storage)
        {
            _storage = storage;
        }

        private string BasePath => Path.Combine(_storage.AppDataPath, "Dialogues");

        public async Task<NPCDialogue> LoadDialogueAsync(string npcId)
        {
            string fileName = $"{npcId}.json";
            string fullPath = Path.Combine(BasePath, fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Dialogue file not found: {fullPath}");

            var json = await File.ReadAllTextAsync(fullPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<NPCDialogue>(json)
                   ?? throw new InvalidDataException($"Failed to parse dialogue json: {fullPath}");
        }

        public Task<List<string>> GetAvailableDialoguesAsync()
        {
            if (!Directory.Exists(BasePath))
                return Task.FromResult(new List<string>());

            var files = Directory.GetFiles(BasePath, "*.json");
            var names = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
            return Task.FromResult(names);
        }
    }
}
