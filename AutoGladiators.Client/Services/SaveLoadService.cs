using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;
using AutoGladiators.Client.Models;
using System.IO;
using System.Text.Json;

namespace AutoGladiators.Client.Services
{
    public class SaveLoadService
    {
        private static readonly IAppLogger Log = AppLog.For<SaveLoadService>();

        private const string SaveFileName = "autosave.json";

        public void Save(GameData data)
        {
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(SaveFileName, json);
        }

        public GameData Load()
        {
            if (!File.Exists(SaveFileName)) return null;
            string json = File.ReadAllText(SaveFileName);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        public void DeleteSave()
        {
            if (File.Exists(SaveFileName))
                File.Delete(SaveFileName);
        }
    }
}
