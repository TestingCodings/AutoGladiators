using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Reflection;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;



namespace AutoGladiators.Client.Config
{
    public static class ConfigLoader
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Configs");

        public static List<BotTemplate> LoadBotTemplates(string path = "bot_templates.json")
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<BotTemplate>>(json) ?? new();
        }

        public static LevelConfig LoadLevelConfig()
            => LoadConfig<LevelConfig>("level_config.json");

        public static List<Move> LoadMoves()
            => LoadConfig<List<Move>>("moves.json");

        public static List<Item> LoadItems()
            => LoadConfig<List<Item>>("items.json");

        public static List<NPCDialogue> LoadNPCDialogues()
            => LoadConfig<List<NPCDialogue>>("npc_dialogue.json");

        public static T LoadConfig<T>(string fileName)
        {
            var fullPath = Path.Combine(ConfigPath, fileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Missing config file: {fullPath}");

            var json = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }
    }
}
