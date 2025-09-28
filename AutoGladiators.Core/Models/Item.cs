using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Models;

public class Item
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; } // e.g., "Healing", "Capture", "Upgrade"
    public int Quantity { get; set; }
    public required string Effect { get; set; } // e.g., "Restores 50 HP", "Increases Attack by 10"

    public void ApplyTo(GladiatorBot bot)
    {
        if (bot.IsFainted)
            return; // Cannot use items on a KO'd bot

        // Example effect application logic
        if (Type == "Healing" && Effect.StartsWith("Restores"))
        {
            var parts = Effect.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int healAmount))
            {
                bot.Heal(healAmount);
                Quantity = Math.Max(0, Quantity - 1);
            }
        }
        else if (Type == "Upgrade" && Effect.StartsWith("Increases"))
        {
            var parts = Effect.Split(' ');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int increaseAmount))
            {
                if (Effect.Contains("Attack"))
                    bot.AttackPower += increaseAmount;
                else if (Effect.Contains("Defense"))
                    bot.Defense += increaseAmount;
                Quantity = Math.Max(0, Quantity - 1);
            }
        }
    }
    // Additional item logic can be added here
}


