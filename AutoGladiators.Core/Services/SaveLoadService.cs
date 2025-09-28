using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;
using AutoGladiators.Core.Models;
using System.IO;
using System.Text.Json;

namespace AutoGladiators.Core.Services
{
    public class SaveLoadService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<SaveLoadService>();

        private const string SaveFileName = "autosave.json";

        public void Save(GameData data)
        {
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(SaveFileName, json);
        }

        public GameData? Load()
        {
            if (!File.Exists(SaveFileName)) return null;
            string json = File.ReadAllText(SaveFileName);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        // Static methods for PlayerProfile (used by tests)
        public static void SaveProfile(PlayerProfile profile, string path)
        {
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static PlayerProfile LoadProfile(string path)
        {
            if (!File.Exists(path)) 
                throw new FileNotFoundException($"Profile file not found: {path}");
                
            string json = File.ReadAllText(path);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(json);
            return profile ?? throw new InvalidDataException("Failed to deserialize profile data");
        }

        public void DeleteSave()
        {
            if (File.Exists(SaveFileName))
                File.Delete(SaveFileName);
        }
    }
}
