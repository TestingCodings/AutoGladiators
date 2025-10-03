using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Services;

var bot = new GladiatorBot
{
    Id = 1,
    Name = "Test Bot",
    Level = 1,
    Experience = 100,
    MaxHealth = 100,
    CurrentHealth = 100,
    AttackPower = 20,
    Defense = 15,
    Speed = 10,
    MaxEnergy = 50,
    Energy = 50,
    Luck = 5,
    ElementalCore = ElementalCore.Fire
};

Console.WriteLine($"Initial: MaxHealth={bot.MaxHealth}, CurrentHealth={bot.CurrentHealth}");

var progressionService = new BotProgressionService();
var result = progressionService.TryLevelUp(bot);

Console.WriteLine($"After level up: HasLeveled={result.HasLeveledUp}");
Console.WriteLine($"Final: MaxHealth={bot.MaxHealth}, CurrentHealth={bot.CurrentHealth}");
Console.WriteLine($"Equal? {bot.CurrentHealth == bot.MaxHealth}");