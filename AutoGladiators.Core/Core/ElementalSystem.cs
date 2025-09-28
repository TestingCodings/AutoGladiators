using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Core
{
    public static class ElementalSystem
    {
        public static double GetModifier(ElementalCore attacker, ElementalCore defender)
        {
            // Example logic
            if (attacker == ElementalCore.Fire && defender == ElementalCore.Grass)
                return 2.0;
            if (attacker == ElementalCore.Grass && defender == ElementalCore.Fire)
                return 0.5;
            return 1.0;
        }

        public static ElementalCore GetElementalCore(string type)
        {
            return type.ToLower() switch
            {
                "fire" => ElementalCore.Fire,
                "water" => ElementalCore.Water,
                "grass" => ElementalCore.Grass,
                "electric" => ElementalCore.Electric,
                "ice" => ElementalCore.Ice,
                _ => ElementalCore.None
            };
        }
    }
}


