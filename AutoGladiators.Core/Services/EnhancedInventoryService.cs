using AutoGladiators.Core.Models;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Rng;
using System;

namespace AutoGladiators.Core.Services
{
    public enum BattleItemType
    {
        HealthPotion,
        MegaHealthPotion,
        EnergyPotion,
        ManaPotion,
        FullRestore,
        RevivePotion,
        StatusCure,
        BattleBooster
    }

    public class BattleItem : InventoryItem
    {
        public BattleItemType BattleType { get; set; }
        public int HealthRestore { get; set; }
        public int ManaRestore { get; set; }
        public int EnergyRestore { get; set; }
        public bool CuresAllStatus { get; set; }
        public bool CanRevive { get; set; }
        public bool CanUseInBattle { get; set; } = true;
        public int UseCooldown { get; set; } = 0; // Turns to wait before using again
        public string AnimationEffect { get; set; } = "";

        public override UseItemResult Use(GladiatorBot target)
        {
            if (target == null)
                return new UseItemResult { Success = false, Message = "Invalid target!" };

            var result = new UseItemResult { Success = true };
            var effects = new List<string>();

            // Health restoration
            if (HealthRestore > 0)
            {
                int actualHealing = Math.Min(HealthRestore, target.MaxHealth - target.CurrentHealth);
                if (actualHealing > 0)
                {
                    target.CurrentHealth += actualHealing;
                    effects.Add($"Restored {actualHealing} HP");
                }
                else if (target.CurrentHealth == target.MaxHealth)
                {
                    effects.Add("Already at full health");
                }
            }

            // Mana restoration
            if (ManaRestore > 0)
            {
                int actualManaRestore = Math.Min(ManaRestore, target.MaxMP - target.CurrentMP);
                if (actualManaRestore > 0)
                {
                    target.CurrentMP += actualManaRestore;
                    effects.Add($"Restored {actualManaRestore} MP");
                }
                else if (target.CurrentMP == target.MaxMP)
                {
                    effects.Add("Already at full MP");
                }
            }

            // Energy restoration
            if (EnergyRestore > 0)
            {
                int actualEnergyRestore = Math.Min(EnergyRestore, target.MaxEnergy - target.Energy);
                if (actualEnergyRestore > 0)
                {
                    target.Energy += actualEnergyRestore;
                    effects.Add($"Restored {actualEnergyRestore} Energy");
                }
            }

            // Revival
            if (CanRevive && target.CurrentHealth == 0)
            {
                target.CurrentHealth = target.MaxHealth / 4; // Revive with 25% health
                effects.Add($"{target.Name} has been revived!");
            }

            // Status cure
            if (CuresAllStatus)
            {
                // Clear status effects (simplified)
                effects.Add("All negative status effects removed");
            }

            // Special battle effects
            ApplySpecialBattleEffects(target, effects);

            result.Message = string.Join(", ", effects);
            return result;
        }

        private void ApplySpecialBattleEffects(GladiatorBot target, List<string> effects)
        {
            switch (BattleType)
            {
                case BattleItemType.BattleBooster:
                    // Temporary combat boosts (would need status effect system)
                    effects.Add("Combat abilities enhanced!");
                    break;

                case BattleItemType.FullRestore:
                    // Full restoration
                    target.CurrentHealth = target.MaxHealth;
                    target.CurrentMP = target.MaxMP;
                    target.Energy = target.MaxEnergy;
                    effects.Clear();
                    effects.Add("Fully restored all stats!");
                    break;
            }
        }

        public string GetItemIcon()
        {
            return BattleType switch
            {
                BattleItemType.HealthPotion => "🧪",
                BattleItemType.MegaHealthPotion => "💉",
                BattleItemType.EnergyPotion => "⚡",
                BattleItemType.ManaPotion => "💙",
                BattleItemType.FullRestore => "✨",
                BattleItemType.RevivePotion => "💖",
                BattleItemType.StatusCure => "🌿",
                BattleItemType.BattleBooster => "🔥",
                _ => "📦"
            };
        }
    }

    public class EnhancedInventoryService : InventoryService
    {
        private readonly Dictionary<BattleItemType, int> _itemUseCooldowns = new();

        public bool CanUseItemInBattle(BattleItemType itemType)
        {
            return !_itemUseCooldowns.ContainsKey(itemType) || _itemUseCooldowns[itemType] <= 0;
        }

        public void UpdateBattleCooldowns()
        {
            var keys = _itemUseCooldowns.Keys.ToList();
            foreach (var key in keys)
            {
                _itemUseCooldowns[key]--;
                if (_itemUseCooldowns[key] <= 0)
                    _itemUseCooldowns.Remove(key);
            }
        }

        public UseItemResult UseBattleItem(BattleItemType itemType, GladiatorBot target)
        {
            if (!CanUseItemInBattle(itemType))
            {
                return new UseItemResult 
                { 
                    Success = false, 
                    Message = "Item is on cooldown!" 
                };
            }

            var item = GetBattleItem(itemType);
            if (item == null)
            {
                return new UseItemResult 
                { 
                    Success = false, 
                    Message = "Item not available!" 
                };
            }

            var result = item.Use(target);
            
            if (result.Success && item.UseCooldown > 0)
            {
                _itemUseCooldowns[itemType] = item.UseCooldown;
            }

            return result;
        }

        private BattleItem? GetBattleItem(BattleItemType itemType)
        {
            return CreateBattleItems().FirstOrDefault(i => i.BattleType == itemType);
        }

        public static List<BattleItem> CreateBattleItems()
        {
            return new List<BattleItem>
            {
                new BattleItem
                {
                    Name = "Health Potion",
                    Description = "Restores 50 HP",
                    BattleType = BattleItemType.HealthPotion,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Common,
                    HealthRestore = 50,
                    AnimationEffect = "healing_sparkles",
                    UseCooldown = 2
                },

                new BattleItem
                {
                    Name = "Mega Health Potion",
                    Description = "Restores 120 HP",
                    BattleType = BattleItemType.MegaHealthPotion,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Rare,
                    HealthRestore = 120,
                    AnimationEffect = "mega_healing",
                    UseCooldown = 3
                },

                new BattleItem
                {
                    Name = "Energy Drink",
                    Description = "Restores 30 Energy",
                    BattleType = BattleItemType.EnergyPotion,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Common,
                    EnergyRestore = 30,
                    AnimationEffect = "energy_burst"
                },

                new BattleItem
                {
                    Name = "Mana Crystal",
                    Description = "Restores 25 MP",
                    BattleType = BattleItemType.ManaPotion,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Uncommon,
                    ManaRestore = 25,
                    AnimationEffect = "mana_swirl",
                    UseCooldown = 1
                },

                new BattleItem
                {
                    Name = "Full Restore",
                    Description = "Completely restores HP, MP, and Energy",
                    BattleType = BattleItemType.FullRestore,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Legendary,
                    HealthRestore = 999,
                    ManaRestore = 999,
                    EnergyRestore = 999,
                    AnimationEffect = "full_restoration",
                    UseCooldown = 5
                },

                new BattleItem
                {
                    Name = "Phoenix Feather",
                    Description = "Revives a defeated gladiator with 25% HP",
                    BattleType = BattleItemType.RevivePotion,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Epic,
                    CanRevive = true,
                    HealthRestore = 1, // Will be calculated as 25% in Use method
                    AnimationEffect = "revival_flame",
                    UseCooldown = 10
                },

                new BattleItem
                {
                    Name = "Panacea",
                    Description = "Cures all status ailments and restores 30 HP",
                    BattleType = BattleItemType.StatusCure,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Rare,
                    HealthRestore = 30,
                    CuresAllStatus = true,
                    AnimationEffect = "purification",
                    UseCooldown = 3
                },

                new BattleItem
                {
                    Name = "Battle Stimulant",
                    Description = "Temporarily boosts combat performance",
                    BattleType = BattleItemType.BattleBooster,
                    Type = ItemType.Consumable,
                    Rarity = ItemRarity.Uncommon,
                    AnimationEffect = "power_boost",
                    UseCooldown = 4
                }
            };
        }

        public List<BattleItem> GetAvailableBattleItems()
        {
            // In a full implementation, this would check actual inventory
            // For now, return a selection of available items
            return CreateBattleItems().Take(5).ToList();
        }
    }
}