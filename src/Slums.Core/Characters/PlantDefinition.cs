using Slums.Core.World;

namespace Slums.Core.Characters;

public sealed record PlantDefinition
{
    public PlantType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ArabicName { get; init; } = string.Empty;

    public PlantCategory Category { get; init; }

    public string Description { get; init; } = string.Empty;

    public int OneTimeCost { get; init; }

    public int WeeklyCareCost { get; init; }

    public int PassiveMotherHealthBonus { get; init; }

    public int ActiveCareMotherHealthBonus { get; init; }

    public int MotherHealthBonusPerUpgrade { get; init; }

    public int CookingBonus { get; init; }

    public int CookingBonusPerUpgrade { get; init; }

    public bool IsSellable { get; init; }

    public int HarvestCycleDays { get; init; }

    public int HarvestSalePrice { get; init; }

    public int HarvestPriceBonusPerUpgrade { get; init; }

    public LocationId PurchaseLocationId { get; init; }
}
