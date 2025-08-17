using System;
using System.Collections.Generic;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services
{
    public class ItemService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<ItemService>();

        public static ItemService Instance { get; } = new();

        private readonly Dictionary<string, Item> _itemDatabase;

        private ItemService()
        {
            _itemDatabase = new Dictionary<string, Item>
            {
                ["repair_kit"] = new Item("repair_kit", "Repair Kit", "Heals 20 HP", (bot) => bot.Heal(20)),
                ["mega_repair"] = new Item("mega_repair", "Mega Repair", "Fully heals bot", (bot) => bot.CurrentHealth = bot.MaxHealth),
                ["attack_boost"] = new Item("attack_boost", "Attack Boost", "Increases strength temporarily", (bot) => bot.Strength += 5)
            };
        }

        public Item? GetItemById(string id)
        {
            _itemDatabase.TryGetValue(id, out var item);
            return item;
        }

        public List<Item> GetAllItems()
        {
            return new List<Item>(_itemDatabase.Values);
        }
    }

    public class Item
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<ItemService>();

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Action<GladiatorBot> Effect { get; }

        public Item(string id, string name, string description, Action<GladiatorBot> effect)
        {
            Id = id;
            Name = name;
            Description = description;
            Effect = effect;
        }

        public void ApplyTo(GladiatorBot bot) => Effect?.Invoke(bot);
    }
}

