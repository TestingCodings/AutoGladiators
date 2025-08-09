using System.Collections.Generic;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.Core
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public List<GladiatorBot> OwnedBots { get; set; } = new();
        public Dictionary<string, int> Inventory { get; set; } = new(); // ItemId -> Quantity

        public void AddItem(string itemId, int quantity = 1)
        {
            if (Inventory.ContainsKey(itemId))
                Inventory[itemId] += quantity;
            else
                Inventory[itemId] = quantity;
        }

        public bool ConsumeItem(string itemId)
        {
            if (Inventory.ContainsKey(itemId) && Inventory[itemId] > 0)
            {
                Inventory[itemId]--;
                if (Inventory[itemId] <= 0)
                    Inventory.Remove(itemId);
                return true;
            }
            return false;
        }

        public void DiscardItem(string itemId)
        {
            if (Inventory.ContainsKey(itemId))
                Inventory.Remove(itemId);
        }

        public int GetItemQuantity(string itemId)
        {
            return Inventory.TryGetValue(itemId, out var count) ? count : 0;
        }
    }
}
