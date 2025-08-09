namespace AutoGladiators.Client.Enums
{
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Electric,
        Earth,
        Wind,
        Metal,
        Plasma,
        Ice
    }

    public enum MoveType
    {
        Physical,
        Special,
        Status
    }

    public enum MoveCategory
    {
        Attack,
        Buff,
        Debuff,
        Heal,
        Capture,
        Escape
    }

    public enum BattleOutcome
    {
        None,
        Victory,
        Defeat,
        Draw,
        Escaped
    }

    public enum StatusEffectType
    {
        None,
        Burn,
        Freeze,
        Stun,
        Poison,
        Sleep,
        Confusion,
        Paralysis,
        Shielded,
        Regeneration
    }

    public enum AIBehaviorType
    {
        Aggressive,
        Defensive,
        Balanced,
        Random,
        Healer
    }

    public enum BotClass
    {
        Scout,
        Brawler,
        Sniper,
        Medic,
        Tank,
        Support,
        Assassin
    }

    public enum ItemType
    {
        Healing,
        Buff,
        Evolution,
        Capture,
        Currency,
        Misc
    }

    public enum DialogueActionType
    {
        None,
        Battle,
        GiftItem,
        Heal,
        End,
        Conditional,
        SetFlag,
        TriggerQuest,
        Warp
    }

    public enum EncounterType
    {
        None,
        WildBot,
        NPC,
        Item,
        Event
    }

    public enum GameMode
    {
        Simulation,
        TurnBased,
        Adventure
    }
}
