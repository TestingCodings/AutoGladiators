using AutoGladiators.Core.Core;


namespace AutoGladiators.Core.Models
{
    public class Inventory
    {
        public List<Item> Items { get; set; } = new();
        public List<GladiatorBot> Bots { get; set; } = new();

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
        public void Clear()
        {
            Items.Clear();
            Bots.Clear();
        }

    }

    
}
