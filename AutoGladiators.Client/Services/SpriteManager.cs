using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Graphics;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Client.Services
{
    /// <summary>
    /// Manages bot sprites, avatars, and visual assets for the game
    /// Provides dynamic sprite generation based on elemental cores and rarity
    /// </summary>
    public class SpriteManager
    {
        private readonly Dictionary<string, ImageSource> _spriteCache;
        private readonly Dictionary<ElementalCore, Color> _elementalColors;
        private readonly Dictionary<string, Color> _rarityColors;
        private readonly Random _random;

        public SpriteManager()
        {
            _spriteCache = new Dictionary<string, ImageSource>();
            _random = new Random();
            
            // Initialize elemental color mapping
            _elementalColors = new Dictionary<ElementalCore, Color>
            {
                { ElementalCore.Fire, Color.FromArgb("#FF4444") },
                { ElementalCore.Electric, Color.FromArgb("#FFD700") },
                { ElementalCore.Ice, Color.FromArgb("#4A9EFF") },
                { ElementalCore.Metal, Color.FromArgb("#C0C0C0") },
                { ElementalCore.Wind, Color.FromArgb("#00FF88") }
            };

            // Initialize rarity color mapping
            _rarityColors = new Dictionary<string, Color>
            {
                { "Common", Color.FromArgb("#B0B0B0") },
                { "Uncommon", Color.FromArgb("#00FF88") },
                { "Rare", Color.FromArgb("#4A9EFF") },
                { "Epic", Color.FromArgb("#8B5CF6") },
                { "Legendary", Color.FromArgb("#FFD700") }
            };
        }

        /// <summary>
        /// Gets or generates a bot avatar based on elemental core and rarity
        /// </summary>
        public ImageSource GetBotAvatar(string botName, ElementalCore element, string rarity, int level)
        {
            string cacheKey = $"{botName}_{element}_{rarity}_{level}";
            
            if (_spriteCache.ContainsKey(cacheKey))
            {
                return _spriteCache[cacheKey];
            }

            var avatar = GenerateBotAvatar(element, rarity, level);
            _spriteCache[cacheKey] = avatar;
            return avatar;
        }

        /// <summary>
        /// Gets elemental icon for UI elements
        /// </summary>
        public string GetElementalIcon(ElementalCore element)
        {
            return element switch
            {
                ElementalCore.Fire => "🔥",
                ElementalCore.Electric => "⚡",
                ElementalCore.Ice => "❄️",
                ElementalCore.Metal => "⚙️",
                ElementalCore.Wind => "💨",
                _ => "⚡"
            };
        }

        /// <summary>
        /// Gets rarity icon for UI elements
        /// </summary>
        public string GetRarityIcon(string rarity)
        {
            return rarity switch
            {
                "Common" => "⚪",
                "Uncommon" => "🟢",
                "Rare" => "🔵",
                "Epic" => "🟣",
                "Legendary" => "🟡",
                _ => "⚪"
            };
        }

        /// <summary>
        /// Gets elemental color for UI theming
        /// </summary>
        public Color GetElementalColor(ElementalCore element)
        {
            return _elementalColors.GetValueOrDefault(element, Color.FromArgb("#4A9EFF"));
        }

        /// <summary>
        /// Gets rarity color for UI theming
        /// </summary>
        public Color GetRarityColor(string rarity)
        {
            return _rarityColors.GetValueOrDefault(rarity, Color.FromArgb("#B0B0B0"));
        }

        /// <summary>
        /// Generates equipment icon based on slot type
        /// </summary>
        public string GetEquipmentIcon(string equipmentType)
        {
            return equipmentType switch
            {
                "Weapon" => "⚔️",
                "Armor" => "🛡️",
                "Accessory" => "💍",
                "Module" => "🔧",
                _ => "📦"
            };
        }

        /// <summary>
        /// Gets battle effect icon for abilities
        /// </summary>
        public string GetBattleEffectIcon(string effectType)
        {
            return effectType switch
            {
                "Damage" => "💥",
                "Heal" => "💚",
                "Shield" => "🛡️",
                "Buff" => "⬆️",
                "Debuff" => "⬇️",
                "Stun" => "💫",
                "Burn" => "🔥",
                "Freeze" => "🧊",
                "Shock" => "⚡",
                _ => "✨"
            };
        }

        /// <summary>
        /// Generates a procedural bot avatar (placeholder for actual sprite generation)
        /// In a production environment, this would load actual sprite assets
        /// </summary>
        private ImageSource GenerateBotAvatar(ElementalCore element, string rarity, int level)
        {
            // For now, return a placeholder ImageSource
            // In production, this would:
            // 1. Load base bot sprite templates
            // 2. Apply elemental color overlays
            // 3. Add rarity effects (glow, particles)
            // 4. Scale based on level
            // 5. Cache the result
            
            return ImageSource.FromFile("dotnet_bot.png"); // Placeholder
        }

        /// <summary>
        /// Preloads commonly used sprites for performance
        /// </summary>
        public async Task PreloadCommonSprites()
        {
            var commonElements = new[] { ElementalCore.Fire, ElementalCore.Electric, ElementalCore.Ice };
            var commonRarities = new[] { "Common", "Uncommon", "Rare" };
            
            foreach (var element in commonElements)
            {
                foreach (var rarity in commonRarities)
                {
                    for (int level = 1; level <= 5; level++)
                    {
                        GetBotAvatar($"Bot_{element}_{rarity}", element, rarity, level);
                    }
                }
            }
        }

        /// <summary>
        /// Clears sprite cache to free memory
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
        }

        /// <summary>
        /// Gets cache statistics for debugging
        /// </summary>
        public (int Count, long EstimatedMemory) GetCacheStats()
        {
            return (_spriteCache.Count, _spriteCache.Count * 1024); // Rough estimate
        }
    }
}