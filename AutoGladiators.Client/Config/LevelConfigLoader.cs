using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AutoGladiators.Client.Config
{
    public static class LevelConfigLoader
    {
        public static List<LevelConfig> LoadAllLevels(string path)
        {
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            var levels = new List<LevelConfig>();
            foreach (var levelElement in root.GetProperty("levels").EnumerateArray())
            {
                var json = levelElement.GetRawText();
                var level = JsonSerializer.Deserialize<LevelConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (level != null)
                    levels.Add(level);
            }

            return levels;
        }
    }
}
