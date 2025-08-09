using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;


namespace AutoGladiators.Client.Services {
    public class InventoryService
    {
        public static InventoryService Instance { get; } = new();

        private PlayerProfile _profile => GameStateService.Instance.CurrentPlayer;

        public List<ItemDisplay> GetInventory()
        {
            var items = new List<ItemDisplay>();
            foreach (var item in GameStateService.Instance.Inventory.Items)
            {
                items.Add(new ItemDisplay(item, 1)); // Quantity logic can be improved if needed
            }
            return items;
        }

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

    public class ItemDisplay(AutoGladiators.Client.Models.Item item, int quantity)
    {
        public string Name => Item.Name;
        public string Description => Item.Description;
        public int Quantity { get; set; } = quantity;

        public AutoGladiators.Client.Models.Item Item { get; } = item;
    }
}

