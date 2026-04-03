namespace Slums.Core.Characters;

public sealed class HouseholdAssetsState
{
    public const int MaxCats = 3;
    public const int MaxPlants = 10;
    private const int MotherHealthBonusCap = 8;
    private const int HomeCookingBonusCap = 3;
    private readonly List<OwnedPet> _pets = [];
    private readonly List<OwnedPlant> _plants = [];

    public IReadOnlyList<OwnedPet> Pets => _pets;

    public IReadOnlyList<OwnedPlant> Plants => _plants;

    public bool HasStreetCatEncounter { get; private set; }

    public int LastStreetCatEncounterDay { get; private set; }

    public int TotalHerbEarnings { get; private set; }

    public bool HasAnyAssets => _pets.Count > 0 || _plants.Count > 0;

    public bool CanAdoptCat => HasStreetCatEncounter && _pets.Count(static pet => pet.Type == PetType.Cat) < MaxCats;

    public bool CanBuyFishTank => _pets.All(static pet => pet.Type != PetType.Fish);

    public bool CanBuyPlant => _plants.Count < MaxPlants;

    public OwnedPet? GetFishTank()
    {
        return _pets.FirstOrDefault(static pet => pet.Type == PetType.Fish);
    }

    public bool TryTriggerStreetCatEncounter(int currentDay)
    {
        if (HasStreetCatEncounter || LastStreetCatEncounterDay == currentDay || _pets.Count(static pet => pet.Type == PetType.Cat) >= MaxCats)
        {
            return false;
        }

        HasStreetCatEncounter = true;
        LastStreetCatEncounterDay = currentDay;
        return true;
    }

    public bool AdoptCat(int currentDay, int currentWeek)
    {
        if (!CanAdoptCat)
        {
            return false;
        }

        _pets.Add(OwnedPet.Create(PetType.Cat, currentDay, currentWeek));
        HasStreetCatEncounter = false;
        return true;
    }

    public bool BuyFishTank(int currentDay, int currentWeek)
    {
        if (!CanBuyFishTank)
        {
            return false;
        }

        _pets.Add(OwnedPet.Create(PetType.Fish, currentDay, currentWeek));
        return true;
    }

    public bool BuyPlant(PlantType type, int currentDay, int currentWeek)
    {
        if (!CanBuyPlant)
        {
            return false;
        }

        _plants.Add(OwnedPlant.Create(type, currentDay, currentWeek));
        return true;
    }

    public OwnedPlant? GetPlant(Guid plantId)
    {
        return _plants.FirstOrDefault(plant => plant.Id == plantId);
    }

    public int GetPetCareCostDue(int currentWeek)
    {
        return _pets
            .Where(pet => !pet.IsUpkeepPaidForWeek(currentWeek))
            .Sum(pet => PetRegistry.GetByType(pet.Type).WeeklyCareCost);
    }

    public int GetPlantCareCostDue(int currentWeek)
    {
        return _plants
            .Where(plant => !plant.IsBaseCarePaidForWeek(currentWeek))
            .Sum(plant => PlantRegistry.GetByType(plant.Type).WeeklyCareCost);
    }

    public bool HasPetCareDue(int currentWeek)
    {
        return _pets.Any(pet => !pet.IsUpkeepPaidForWeek(currentWeek));
    }

    public bool HasPlantCareDue(int currentWeek)
    {
        return _plants.Any(plant => !plant.IsBaseCarePaidForWeek(currentWeek));
    }

    public void PayPetCare(int currentWeek)
    {
        foreach (var pet in _pets)
        {
            pet.PayUpkeep(currentWeek);
        }
    }

    public void PayPlantCare(int currentWeek)
    {
        foreach (var plant in _plants)
        {
            plant.PayBaseCare(currentWeek);
        }
    }

    public bool TryUpgradePlant(Guid plantId, PlantUpgradeType upgradeType, int currentWeek)
    {
        var plant = GetPlant(plantId);
        if (plant is null || !plant.CanPurchaseUpgrade(upgradeType, currentWeek))
        {
            return false;
        }

        plant.PurchaseUpgrade(upgradeType, currentWeek);
        return true;
    }

    public bool TryUpgradeFishTank(FishTankUpgradeType upgradeType, int currentWeek)
    {
        var fishTank = GetFishTank();
        if (fishTank is null || !fishTank.CanPurchaseUpgrade(upgradeType, currentWeek))
        {
            return false;
        }

        fishTank.PurchaseUpgrade(upgradeType, currentWeek);
        return true;
    }

    public int GetMotherDailyHealthBonus(int currentWeek)
    {
        var total = 0;

        foreach (var pet in _pets)
        {
            var definition = PetRegistry.GetByType(pet.Type);
            total += definition.PassiveMotherHealthBonus;
            if (pet.IsUpkeepPaidForWeek(currentWeek))
            {
                total += definition.ActiveCareMotherHealthBonus;
            }

            if (pet.Type == PetType.Fish)
            {
                total += FishTankUpgradeCatalog.GetMotherHealthBonusPerUpgrade * pet.GetActiveUpgradeCount(currentWeek);
            }
        }

        foreach (var plant in _plants)
        {
            var definition = PlantRegistry.GetByType(plant.Type);
            total += definition.PassiveMotherHealthBonus;
            if (plant.IsBaseCarePaidForWeek(currentWeek))
            {
                total += definition.ActiveCareMotherHealthBonus;
            }

            total += definition.MotherHealthBonusPerUpgrade * plant.GetActiveUpgradeCount(currentWeek);
        }

        return Math.Min(MotherHealthBonusCap, total);
    }

    public int GetHomeCookingBonus(int currentWeek)
    {
        var total = 0;

        foreach (var plant in _plants)
        {
            var definition = PlantRegistry.GetByType(plant.Type);
            total += definition.CookingBonus;
            total += definition.CookingBonusPerUpgrade * plant.GetActiveUpgradeCount(currentWeek);
        }

        return Math.Min(HomeCookingBonusCap, total);
    }

    public HouseholdAssetsWeeklyResolution ResolveWeeklyNeglect(int currentWeek)
    {
        var previousWeek = currentWeek - 1;
        if (previousWeek <= 0)
        {
            return new HouseholdAssetsWeeklyResolution(0, 0, 0);
        }

        var unpaidPetCount = _pets.Count(pet => pet.LastUpkeepPaidWeek < previousWeek);
        var unpaidPlantCount = _plants.Count(plant => plant.LastBaseCarePaidWeek < previousWeek);
        var penalty = Math.Min(10, (unpaidPetCount * 2) + unpaidPlantCount);
        return new HouseholdAssetsWeeklyResolution(penalty, unpaidPetCount, unpaidPlantCount);
    }

    public int ResolveSellablePlantIncome(int currentDay, int currentWeek)
    {
        var totalIncome = 0;
        foreach (var plant in _plants)
        {
            var definition = PlantRegistry.GetByType(plant.Type);
            if (plant.TryResolveHarvest(currentDay, currentWeek, definition, out var income))
            {
                totalIncome += income;
            }
        }

        TotalHerbEarnings += totalIncome;
        return totalIncome;
    }

    public void Restore(
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings)
    {
        ArgumentNullException.ThrowIfNull(pets);
        ArgumentNullException.ThrowIfNull(plants);

        _pets.Clear();
        _pets.AddRange(pets);
        _plants.Clear();
        _plants.AddRange(plants);
        HasStreetCatEncounter = hasStreetCatEncounter;
        LastStreetCatEncounterDay = lastStreetCatEncounterDay;
        TotalHerbEarnings = totalHerbEarnings;
    }
}
