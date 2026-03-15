using Slums.Core.World;

namespace Slums.Core.Characters;

public sealed record PetDefinition
{
    public PetType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int MaxOwned { get; init; }

    public int OneTimeCost { get; init; }

    public int WeeklyCareCost { get; init; }

    public int PassiveMotherHealthBonus { get; init; }

    public int ActiveCareMotherHealthBonus { get; init; }

    public LocationId? PurchaseLocationId { get; init; }

    public bool RequiresStreetEncounter { get; init; }
}
