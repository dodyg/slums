using Slums.Core.World;

namespace Slums.Core.Characters;

public static class PlantRegistry
{
    private static readonly IReadOnlyList<PlantDefinition> DefaultDefinitions =
    [
        CreateCulinary(PlantType.Basil, "Basil", "raihan", 10),
        CreateCulinary(PlantType.Mint, "Mint", "na'na'", 11),
        CreateCulinary(PlantType.Parsley, "Parsley", "baqdounis", 9),
        CreateCulinary(PlantType.Coriander, "Coriander", "kozbara", 9),
        CreateCulinary(PlantType.Dill, "Dill", "shibitt", 8),
        new PlantDefinition
        {
            Type = PlantType.Chamomile,
            Name = "Chamomile",
            ArabicName = "babounig",
            Category = PlantCategory.SellableHerb,
            Description = "A hardy herb you can dry and sell through a nearby street vendor.",
            OneTimeCost = 12,
            WeeklyCareCost = 3,
            CookingBonus = 1,
            CookingBonusPerUpgrade = 1,
            IsSellable = true,
            HarvestCycleDays = 5,
            HarvestSalePrice = 10,
            HarvestPriceBonusPerUpgrade = 5,
            PurchaseLocationId = LocationId.PlantShop
        },
        new PlantDefinition
        {
            Type = PlantType.Hibiscus,
            Name = "Hibiscus",
            ArabicName = "karkade",
            Category = PlantCategory.SellableHerb,
            Description = "A stronger household seller with a longer cycle and better return.",
            OneTimeCost = 18,
            WeeklyCareCost = 4,
            CookingBonus = 1,
            CookingBonusPerUpgrade = 1,
            IsSellable = true,
            HarvestCycleDays = 7,
            HarvestSalePrice = 25,
            HarvestPriceBonusPerUpgrade = 12,
            PurchaseLocationId = LocationId.PlantShop
        },
        CreateFlower(PlantType.Jasmine, "Jasmine", "yasmin", 14),
        CreateFlower(PlantType.Rose, "Rose", "ward", 16),
        CreateFlower(PlantType.Geranium, "Geranium", "-", 13),
        CreateFlower(PlantType.Bougainvillea, "Bougainvillea", "buganfil", 17),
        CreateFlower(PlantType.Marigold, "Marigold", "tagetes", 12),
        CreateFlower(PlantType.Zinnia, "Zinnia", "-", 11),
        CreateFlower(PlantType.Petunia, "Petunia", "-", 11),
        new PlantDefinition
        {
            Type = PlantType.AloeVera,
            Name = "Aloe Vera",
            ArabicName = "saber",
            Category = PlantCategory.Medicinal,
            Description = "A medicinal plant that quietly supports your mother's health.",
            OneTimeCost = 20,
            WeeklyCareCost = 4,
            PassiveMotherHealthBonus = 2,
            ActiveCareMotherHealthBonus = 1,
            MotherHealthBonusPerUpgrade = 1,
            PurchaseLocationId = LocationId.PlantShop
        }
    ];

    private static IReadOnlyList<PlantDefinition> _definitions = DefaultDefinitions;

    public static IReadOnlyList<PlantDefinition> AllDefinitions => _definitions;

    public static void Configure(IEnumerable<PlantDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var configuredDefinitions = definitions.Where(static definition => definition is not null).ToArray();
        if (configuredDefinitions.Length > 0)
        {
            _definitions = configuredDefinitions;
        }
    }

    public static PlantDefinition GetByType(PlantType type)
    {
        return _definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No plant definition configured for {type}.");
    }

    private static PlantDefinition CreateCulinary(PlantType type, string name, string arabicName, int cost)
    {
        return new PlantDefinition
        {
            Type = type,
            Name = name,
            ArabicName = arabicName,
            Category = PlantCategory.CulinaryHerb,
            Description = "A kitchen herb that stretches simple meals a little further.",
            OneTimeCost = cost,
            WeeklyCareCost = 3,
            CookingBonus = 1,
            CookingBonusPerUpgrade = 1,
            PurchaseLocationId = LocationId.PlantShop
        };
    }

    private static PlantDefinition CreateFlower(PlantType type, string name, string arabicName, int cost)
    {
        return new PlantDefinition
        {
            Type = type,
            Name = name,
            ArabicName = arabicName,
            Category = PlantCategory.Flower,
            Description = "A decorative plant that softens the room and lifts your mother's spirits.",
            OneTimeCost = cost,
            WeeklyCareCost = 3,
            PassiveMotherHealthBonus = 1,
            ActiveCareMotherHealthBonus = 1,
            MotherHealthBonusPerUpgrade = 1,
            PurchaseLocationId = LocationId.PlantShop
        };
    }
}
