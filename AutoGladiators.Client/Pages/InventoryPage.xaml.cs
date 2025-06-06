using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace AutoGladiators.Client.Pages
{
    public partial class InventoryPage : ContentPage
    {
        public class InventoryItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }

            public string QuantityText => $"Quantity: {Quantity}";
        }

        public InventoryPage()
        {
            InitializeComponent();

            ItemList.ItemsSource = new List<InventoryItem>
            {
                new InventoryItem { Name = "Control Chip", Description = "Used to capture bots in battle.", Quantity = 3 },
                new InventoryItem { Name = "Repair Kit", Description = "Restores 30 HP to your bot.", Quantity = 5 },
                new InventoryItem { Name = "Energy Core", Description = "Boosts bot energy by 20.", Quantity = 2 }
            };
        }
    }
}
