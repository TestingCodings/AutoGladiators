using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services {
    
    public enum ItemType
    {
        Healing,
        StatBooster,
        CaptureDevice,
        Equipment,
        Consumable
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public class UseItemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }

        public static UseItemResult CreateSuccess(string message, int value = 0)
        {
            return new UseItemResult { Success = true, Message = message, Value = value };
        }

        public static UseItemResult CreateFailed(string message)
        {
            return new UseItemResult { Success = false, Message = message, Value = 0 };
        }
    }

    public abstract class InventoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Value { get; set; }
        public bool IsConsumable { get; set; } = true;
        
        public abstract UseItemResult Use(GladiatorBot target);
        
        public virtual bool CanUse(GladiatorBot target) => target != null;
    }

    public class HealingPotion : InventoryItem
    {
        public int HealAmount { get; set; }

        public HealingPotion(int healAmount = 50)
        {
            Name = "Healing Potion";
            Description = $"Restores {healAmount} health points";
            Type = ItemType.Healing;
            Rarity = ItemRarity.Common;
            Value = healAmount / 2;
            HealAmount = healAmount;
        }

        public override UseItemResult Use(GladiatorBot target)
        {
            if (!CanUse(target))
                return UseItemResult.CreateFailed("Cannot use healing potion on this target");

            if (target.CurrentHealth >= target.MaxHealth)
                return UseItemResult.CreateFailed("Bot is already at full health");

            int previousHealth = target.CurrentHealth;
            target.CurrentHealth = Math.Min(target.MaxHealth, target.CurrentHealth + HealAmount);
            int actualHeal = target.CurrentHealth - previousHealth;

            return UseItemResult.CreateSuccess($"Healed {actualHeal} HP", actualHeal);
        }

        public override bool CanUse(GladiatorBot target)
        {
            return base.CanUse(target) && target.CurrentHealth < target.MaxHealth;
        }
    }

    public class EnergyPotion : InventoryItem
    {
        public int EnergyAmount { get; set; }

        public EnergyPotion(int energyAmount = 30)
        {
            Name = "Energy Drink";
            Description = $"Restores {energyAmount} energy points";
            Type = ItemType.Consumable;
            Rarity = ItemRarity.Common;
            Value = energyAmount / 3;
            EnergyAmount = energyAmount;
        }

        public override UseItemResult Use(GladiatorBot target)
        {
            if (!CanUse(target))
                return UseItemResult.CreateFailed("Cannot use energy potion on this target");

            if (target.Energy >= target.MaxEnergy)
                return UseItemResult.CreateFailed("Bot is already at full energy");

            int previousEnergy = target.Energy;
            target.Energy = Math.Min(target.MaxEnergy, target.Energy + EnergyAmount);
            int actualRestore = target.Energy - previousEnergy;

            return UseItemResult.CreateSuccess($"Restored {actualRestore} Energy", actualRestore);
        }

        public override bool CanUse(GladiatorBot target)
        {
            return base.CanUse(target) && target.Energy < target.MaxEnergy;
        }
    }

    public enum StatType
    {
        Health,
        Attack,
        Defense,
        Speed,
        Energy,
        Luck
    }

    public class StatBooster : InventoryItem
    {
        public StatType StatType { get; set; }
        public int BoostAmount { get; set; }
        public bool IsPermanent { get; set; }

        public StatBooster(StatType statType, int boostAmount, bool isPermanent = false)
        {
            StatType = statType;
            BoostAmount = boostAmount;
            IsPermanent = isPermanent;
            
            Name = $"{statType} {(isPermanent ? "Enhancer" : "Booster")}";
            Description = $"{(isPermanent ? "Permanently increases" : "Temporarily boosts")} {statType} by {boostAmount}";
            Type = ItemType.StatBooster;
            Rarity = isPermanent ? ItemRarity.Rare : ItemRarity.Uncommon;
            Value = boostAmount * (isPermanent ? 10 : 3);
        }

        public override UseItemResult Use(GladiatorBot target)
        {
            if (!CanUse(target))
                return UseItemResult.CreateFailed("Cannot use stat booster on this target");

            switch (StatType)
            {
                case StatType.Attack:
                    target.AttackPower += BoostAmount;
                    break;
                case StatType.Defense:
                    target.Defense += BoostAmount;
                    break;
                case StatType.Speed:
                    target.Speed += BoostAmount;
                    break;
                case StatType.Health:
                    target.MaxHealth += BoostAmount;
                    if (IsPermanent) target.CurrentHealth += BoostAmount;
                    break;
                case StatType.Energy:
                    target.MaxEnergy += BoostAmount;
                    if (IsPermanent) target.Energy += BoostAmount;
                    break;
                case StatType.Luck:
                    target.Luck += BoostAmount;
                    break;
            }

            string result = IsPermanent 
                ? $"Permanently increased {StatType} by {BoostAmount}"
                : $"Boosted {StatType} by {BoostAmount}";
                
            return UseItemResult.CreateSuccess(result, BoostAmount);
        }
    }

    public class CaptureDevice : InventoryItem
    {
        public double CaptureRate { get; set; }
        public int MaxUses { get; set; }
        public int RemainingUses { get; set; }

        public CaptureDevice(double captureRate = 0.3, int maxUses = 5)
        {
            Name = "Capture Orb";
            Description = $"Attempts to capture defeated bots ({captureRate:P0} base rate)";
            Type = ItemType.CaptureDevice;
            Rarity = ItemRarity.Uncommon;
            Value = 100;
            CaptureRate = captureRate;
            MaxUses = maxUses;
            RemainingUses = maxUses;
        }

        public override UseItemResult Use(GladiatorBot target)
        {
            if (RemainingUses <= 0)
                return UseItemResult.CreateFailed("Capture device is depleted");

            if (target.CurrentHealth > target.MaxHealth * 0.1) // Can only capture when HP < 10%
                return UseItemResult.CreateFailed("Target must be weakened before capture attempt");

            RemainingUses--;
            
            // Calculate capture chance based on health and luck
            double healthFactor = 1.0 - ((double)target.CurrentHealth / target.MaxHealth);
            double finalCaptureRate = CaptureRate * healthFactor * 2.0; // Double rate when very weak
            
            var random = new Random();
            bool captured = random.NextDouble() < finalCaptureRate;

            if (captured)
            {
                return UseItemResult.CreateSuccess($"Successfully captured {target.Name}!", 1);
            }
            else
            {
                return UseItemResult.CreateSuccess($"Capture failed! {RemainingUses} uses remaining", 0);
            }
        }

        public override bool CanUse(GladiatorBot target)
        {
            return base.CanUse(target) && RemainingUses > 0;
        }
    }

    public class InventoryService
    {
        private static readonly IAppLogger _logger = AppLog.For<InventoryService>();

        public static InventoryService Instance { get; } = new();

        private readonly Dictionary<Guid, (InventoryItem Item, int Quantity)> _modernInventory = new();

        private PlayerProfile _profile => GameStateService.Instance.CurrentPlayer;

        public List<ItemDisplay> GetInventory()
        {
            var items = new List<ItemDisplay>();
            
            // Legacy item support
            foreach (var item in GameStateService.Instance.Inventory.Items)
            {
                items.Add(new ItemDisplay(item, 1));
            }

            // Modern inventory items
            foreach (var (item, quantity) in _modernInventory.Values)
            {
                items.Add(new ItemDisplay(item, quantity));
            }
            
            return items;
        }

        public void AddItem(InventoryItem item, int quantity = 1)
        {
            if (_modernInventory.ContainsKey(item.Id))
            {
                var current = _modernInventory[item.Id];
                _modernInventory[item.Id] = (current.Item, current.Quantity + quantity);
            }
            else
            {
                _modernInventory[item.Id] = (item, quantity);
            }

            _logger.LogUserAction("ItemAdded", item.Id.ToString(), new Dictionary<string, object?>
            {
                ["ItemName"] = item.Name,
                ["ItemType"] = item.Type.ToString(),
                ["Quantity"] = quantity,
                ["TotalQuantity"] = _modernInventory[item.Id].Quantity
            });
        }

        public UseItemResult UseModernItem(Guid itemId, GladiatorBot target)
        {
            if (!_modernInventory.ContainsKey(itemId))
                return UseItemResult.CreateFailed("Item not found in inventory");

            var (item, quantity) = _modernInventory[itemId];
            
            if (!item.CanUse(target))
                return UseItemResult.CreateFailed($"Cannot use {item.Name} on this target");

            var result = item.Use(target);
            
            if (result.Success && item.IsConsumable)
            {
                RemoveItem(itemId, 1);
            }

            _logger.LogUserAction("ItemUsed", itemId.ToString(), new Dictionary<string, object?>
            {
                ["ItemName"] = item.Name,
                ["TargetBot"] = target.Name,
                ["Success"] = result.Success,
                ["Result"] = result.Message
            });

            return result;
        }

        public bool RemoveItem(Guid itemId, int quantity = 1)
        {
            if (!_modernInventory.ContainsKey(itemId))
                return false;

            var current = _modernInventory[itemId];
            if (current.Quantity < quantity)
                return false;

            if (current.Quantity == quantity)
            {
                _modernInventory.Remove(itemId);
            }
            else
            {
                _modernInventory[itemId] = (current.Item, current.Quantity - quantity);
            }

            return true;
        }

        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            return _modernInventory.Values
                .Where(x => x.Item.Type == type)
                .Select(x => x.Item)
                .ToList();
        }

        // Legacy support methods
        public bool UseItem(string itemId, GladiatorBot targetBot)
        {
            var item = ItemService.Instance.GetItemById(itemId);
            if (item == null) return false;

            item.ApplyTo(targetBot);
            // Remove the item from inventory after use
            var inventory = GameStateService.Instance.Inventory.Items;
            var toRemove = inventory.Find(i => i.Name == item.Name);
            if (toRemove != null)
            {
                inventory.Remove(toRemove);
                return true;
            }
            return false;
        }

        public void DiscardItem(string itemId)
        {
            var inventory = GameStateService.Instance.Inventory.Items;
            var toRemove = inventory.Find(i => i.Name == itemId);
            if (toRemove != null)
            {
                inventory.Remove(toRemove);
            }
        }
    }

    public class ItemDisplay
    {
        private static readonly IAppLogger Log = AppLog.For<ItemDisplay>();

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public AutoGladiators.Core.Models.Item? LegacyItem { get; set; }
        public InventoryItem? ModernItem { get; set; }

        public ItemDisplay(AutoGladiators.Core.Models.Item item, int quantity)
        {
            Name = item.Name;
            Description = item.Description;
            Quantity = quantity;
            LegacyItem = item;
        }

        public ItemDisplay(InventoryItem item, int quantity)
        {
            Name = item.Name;
            Description = item.Description;
            Quantity = quantity;
            ModernItem = item;
        }
    }
}


