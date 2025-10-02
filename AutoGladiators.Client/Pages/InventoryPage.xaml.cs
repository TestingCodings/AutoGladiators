using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class InventoryPage : ContentPage
    {
        public class InventoryItemViewModel
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string RarityColor { get; set; } = "White";
            public ItemDisplay? OriginalItem { get; set; }

            public string QuantityText => $"Quantity: {Quantity}";
        }

        private List<InventoryItemViewModel> _inventoryItems = new();

        public InventoryPage()
        {
            InitializeComponent();
            LoadInventory();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadInventory();
        }

        private void LoadInventory()
        {
            _inventoryItems.Clear();
            
            var inventoryService = InventoryService.Instance;
            var items = inventoryService.GetInventory();

            foreach (var item in items)
            {
                var viewModel = new InventoryItemViewModel
                {
                    Name = item.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    OriginalItem = item,
                    RarityColor = GetRarityColor(item)
                };
                _inventoryItems.Add(viewModel);
            }

            // Add some default items for testing if inventory is empty
            if (!_inventoryItems.Any())
            {
                _inventoryItems.AddRange(new[]
                {
                    new InventoryItemViewModel { Name = "Control Chip", Description = "Used to capture bots in battle.", Quantity = 3, RarityColor = "Gold" },
                    new InventoryItemViewModel { Name = "Repair Kit", Description = "Restores 30 HP to your bot.", Quantity = 5, RarityColor = "LightGreen" },
                    new InventoryItemViewModel { Name = "Energy Core", Description = "Boosts bot energy by 20.", Quantity = 2, RarityColor = "LightBlue" }
                });
            }

            ItemList.ItemsSource = _inventoryItems;
        }

        private string GetRarityColor(ItemDisplay item)
        {
            if (item.ModernItem != null)
            {
                return item.ModernItem.Rarity switch
                {
                    ItemRarity.Common => "White",
                    ItemRarity.Uncommon => "LightGreen",
                    ItemRarity.Rare => "LightBlue",
                    ItemRarity.Epic => "Purple",
                    ItemRarity.Legendary => "Gold",
                    _ => "White"
                };
            }
            return "White";
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is InventoryItemViewModel item)
            {
                // Show item use dialog or details
                var action = await DisplayActionSheet($"Use {item.Name}?", "Cancel", null, "Use Item", "View Details");
                
                if (action == "Use Item")
                {
                    await UseItem(item);
                }
                else if (action == "View Details")
                {
                    await DisplayAlert(item.Name, $"{item.Description}\n\nQuantity: {item.Quantity}", "OK");
                }
            }
        }

        private async Task UseItem(InventoryItemViewModel item)
        {
            if (item.OriginalItem?.ModernItem != null)
            {
                var gameState = GameStateService.Instance;
                // Get the first bot as active bot for now - TODO: implement proper active bot selection
                var playerBot = gameState.BotRoster?.FirstOrDefault();
                
                if (playerBot == null)
                {
                    await DisplayAlert("Error", "No active bot selected", "OK");
                    return;
                }

                var result = InventoryService.Instance.UseModernItem(item.OriginalItem.ModernItem.Id, playerBot);
                
                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    LoadInventory(); // Refresh inventory after use
                }
                else
                {
                    await DisplayAlert("Cannot Use Item", result.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Item Use", $"Used {item.Name}!\n\n{item.Description}", "OK");
                // For legacy items, just show a message for now
            }
        }
    }
}
