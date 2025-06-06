using AutoGladiators.Client.Core;


namespace AutoGladiators.Client.Models
{
    public class Inventory
    {
        public List<Item> Items { get; set; }
        public List<GladiatorBot> Bots { get; set; }

        public Inventory()
        {
            Items = new List<Item>();
            Bots = new List<GladiatorBot>();
        }
    }
}
