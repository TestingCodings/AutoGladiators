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
            foreach (var pair in _profile.Inventory)
            {
                var item = ItemService.Instance.GetItemById(pair.Key);
                if (item != null)
                    items.Add(new ItemDisplay(item, pair.Value));
            }
            return items;
        }

        public bool UseItem(string itemId, GladiatorBot targetBot)
        {
            var item = ItemService.Instance.GetItemById(itemId);
            if (item == null) return false;

            item.ApplyTo(targetBot);
            return _profile.ConsumeItem(itemId);
        }

        public void DiscardItem(string itemId)
        {
            _profile.DiscardItem(itemId);
        }
    }

    public class ItemDisplay
    {
        public string Id => Item.Id;
        public string Name => Item.Name;
        public string Description => Item.Description;
        public int Quantity { get; set; }

        public Item Item { get; }

        public ItemDisplay(Item item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }
}
