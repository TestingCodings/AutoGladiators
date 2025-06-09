namespace AutoGladiators.Client.Core
{
    public static class ElementalSystem
    {
        public static double GetModifier(ElementalType attacker, ElementalType defender)
        {
            // Example logic
            if (attacker == ElementalType.Fire && defender == ElementalType.Grass)
                return 2.0;
            if (attacker == ElementalType.Grass && defender == ElementalType.Fire)
                return 0.5;
            return 1.0;
        }

        public static ElementalType GetElementalType(string type)
        {
            return type.ToLower() switch
            {
                "fire" => ElementalType.Fire,
                "water" => ElementalType.Water,
                "grass" => ElementalType.Grass,
                "electric" => ElementalType.Electric,
                "ice" => ElementalType.Ice,
                _ => ElementalType.None
            };
        }
    }

    public enum ElementalType
    {
        None,
        Fire,
        Water,
        Grass,
        Electric,
        Ice
    }
}
