using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Territory;

public enum TerritoryEventType
{
    StreetArgument,
    ProtectionDemand,
    AllianceShift,
    PoliceCrackdown,
    TerritoryFlip,
    RefugeeSolidarity,
    Crossfire
}

public sealed record TerritoryEvent
{
    public TerritoryEventType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public int StressModifier { get; init; }
    public int HealthModifier { get; init; }
    public int MoneyModifier { get; init; }
    public int TensionModifier { get; init; }
    public int InfluenceModifier { get; init; }
    public bool BlocksMarket { get; init; }
    public bool BlocksCrime { get; init; }
    public int DurationDays { get; init; }
    public string? Narration { get; init; }
}

public static class TerritoryEventRegistry
{
    public static TerritoryEvent StreetArgument => new()
    {
        Type = TerritoryEventType.StreetArgument,
        Description = "Street argument",
        StressModifier = 3,
        BlocksMarket = true,
        DurationDays = 1,
        Narration = "Shouting erupts in the street. Two men from different factions face off near the market stalls. Vendors scramble to pack up."
    };

    public static TerritoryEvent CreateProtectionDemand(int cost) => new()
    {
        Type = TerritoryEventType.ProtectionDemand,
        Description = $"Protection demand: {cost} LE",
        StressModifier = 5,
        MoneyModifier = -cost,
        DurationDays = 1,
        Narration = $"A faction enforcer stands at your door. \"{cost} LE for protection. Pay or we remember who isn't family.\""
    };

    public static TerritoryEvent AllianceShift => new()
    {
        Type = TerritoryEventType.AllianceShift,
        Description = "Alliance shift",
        TensionModifier = -10,
        InfluenceModifier = 0,
        DurationDays = 1,
        Narration = "Word spreads fast. The balance of power has shifted in the district. New faces on the corners, old ones gone."
    };

    public static TerritoryEvent PoliceCrackdownEvent => new()
    {
        Type = TerritoryEventType.PoliceCrackdown,
        Description = "Police crackdown",
        StressModifier = 5,
        TensionModifier = -30,
        InfluenceModifier = -10,
        BlocksCrime = true,
        DurationDays = 2,
        Narration = "Overnight, everything changes. Police raids sweep the district. Doors kicked in, people rounded up. The streets empty."
    };

    public static TerritoryEvent TerritoryFlipEvent(FactionId? newFaction) => new()
    {
        Type = TerritoryEventType.TerritoryFlip,
        Description = $"Territory flip: {(newFaction?.ToString() ?? "contested")}",
        StressModifier = 5,
        TensionModifier = -15,
        DurationDays = 3,
        Narration = "The neighborhood has changed hands. The old power is out. Everything feels different now."
    };

    public static TerritoryEvent RefugeeSolidarityEvent => new()
    {
        Type = TerritoryEventType.RefugeeSolidarity,
        Description = "Refugee community solidarity",
        StressModifier = -5,
        TensionModifier = -10,
        DurationDays = 1,
        Narration = "The Sudanese community gathers. Someone brings tea, someone else brings bread. You stand together."
    };

    public static TerritoryEvent CrossfireEvent => new()
    {
        Type = TerritoryEventType.Crossfire,
        Description = "Caught in crossfire",
        StressModifier = 10,
        HealthModifier = -10,
        DurationDays = 1,
        Narration = "Gunshots crack through the alley. You press yourself against the wall, heart hammering. A stray shard catches your arm."
    };
}
