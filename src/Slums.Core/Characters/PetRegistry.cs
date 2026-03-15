using Slums.Core.World;

namespace Slums.Core.Characters;

public static class PetRegistry
{
    private static readonly IReadOnlyList<PetDefinition> DefaultDefinitions =
    [
        new PetDefinition
        {
            Type = PetType.Cat,
            Name = "Street Cat",
            Description = "A stray cat that curls up near your mother and makes the flat feel less empty.",
            MaxOwned = 3,
            OneTimeCost = 0,
            WeeklyCareCost = 6,
            PassiveMotherHealthBonus = 1,
            ActiveCareMotherHealthBonus = 1,
            RequiresStreetEncounter = true
        },
        new PetDefinition
        {
            Type = PetType.Fish,
            Name = "Fish Tank",
            Description = "A small tank from the fish market. It brightens the room, but the food still costs money every week.",
            MaxOwned = 1,
            OneTimeCost = 35,
            WeeklyCareCost = 8,
            PassiveMotherHealthBonus = 1,
            ActiveCareMotherHealthBonus = 1,
            PurchaseLocationId = LocationId.FishMarket
        }
    ];

    private static IReadOnlyList<PetDefinition> _definitions = DefaultDefinitions;

    public static IReadOnlyList<PetDefinition> AllDefinitions => _definitions;

    public static void Configure(IEnumerable<PetDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var configuredDefinitions = definitions.Where(static definition => definition is not null).ToArray();
        if (configuredDefinitions.Length > 0)
        {
            _definitions = configuredDefinitions;
        }
    }

    public static PetDefinition GetByType(PetType type)
    {
        return _definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No pet definition configured for {type}.");
    }
}
