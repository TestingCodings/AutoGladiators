
namespace AutoGladiators.Client.Models
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // e.g., "Healing", "Capture", "Upgrade"
        public int Quantity { get; set; }
    }
}
