using Slums.Core.Characters;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.World;

namespace Slums.Infrastructure.Content;

/// <summary>
/// Validates the repo-owned content catalog during bootstrap. Missing, empty, incomplete,
/// duplicated, or cross-referenced-invalid content fails startup with a precise error instead
/// of silently falling back to hardcoded defaults.
/// </summary>
public static class ContentCatalogValidator
{
    public static void Validate(
        IReadOnlyList<Background> backgrounds,
        IReadOnlyList<Location> locations,
        IReadOnlyList<JobShift> jobs,
        IReadOnlyList<RandomEvent> randomEvents,
        IReadOnlyList<DistrictConditionDefinition> districtConditions,
        IReadOnlyList<PetDefinition> pets,
        IReadOnlyList<PlantDefinition> plants,
        IReadOnlySet<string> knownInkKnots)
    {
        ArgumentNullException.ThrowIfNull(backgrounds);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(jobs);
        ArgumentNullException.ThrowIfNull(randomEvents);
        ArgumentNullException.ThrowIfNull(districtConditions);
        ArgumentNullException.ThrowIfNull(pets);
        ArgumentNullException.ThrowIfNull(plants);
        ArgumentNullException.ThrowIfNull(knownInkKnots);

        var problems = new List<string>();

        ValidateBackgrounds(backgrounds, knownInkKnots, problems);
        ValidateLocations(locations, problems);
        ValidateJobs(jobs, problems);
        ValidateRandomEvents(randomEvents, knownInkKnots, problems);
        ValidateDistrictConditions(districtConditions, problems);
        ValidatePets(pets, locations, problems);
        ValidatePlants(plants, locations, problems);

        if (problems.Count > 0)
        {
            throw new ContentLoadException(
                "Content catalog validation failed:" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", problems));
        }
    }

    private static void ValidateBackgrounds(IReadOnlyList<Background> backgrounds, IReadOnlySet<string> knownInkKnots, List<string> problems)
    {
        if (backgrounds.Count == 0)
        {
            problems.Add("backgrounds: catalog is empty.");
            return;
        }

        var configuredTypes = new HashSet<BackgroundType>();
        foreach (var background in backgrounds)
        {
            if (!configuredTypes.Add(background.Type))
            {
                problems.Add($"backgrounds: duplicate type {background.Type}.");
            }

            if (string.IsNullOrWhiteSpace(background.Name))
            {
                problems.Add($"backgrounds: {background.Type} has no name.");
            }

            if (background.StartingMoney < 0)
            {
                problems.Add($"backgrounds: {background.Type} has negative starting money ({background.StartingMoney}).");
            }

            if (!IsPercentage(background.StartingHealth) || !IsPercentage(background.StartingEnergy) || !IsPercentage(background.StartingHunger) || !IsPercentage(background.StartingStress))
            {
                problems.Add($"backgrounds: {background.Type} has a starting stat outside 0..100.");
            }

            if (!IsPercentage(background.MotherStartingHealth))
            {
                problems.Add($"backgrounds: {background.Type} has mother starting health outside 0..100.");
            }

            if (background.FoodStockpile < 0)
            {
                problems.Add($"backgrounds: {background.Type} has negative food stockpile ({background.FoodStockpile}).");
            }

            if (!string.IsNullOrWhiteSpace(background.InkIntroKnot) && !knownInkKnots.Contains(background.InkIntroKnot))
            {
                problems.Add($"backgrounds: {background.Type} references missing ink knot '{background.InkIntroKnot}'.");
            }
        }

        var missingTypes = Enum.GetValues<BackgroundType>()
            .Where(type => !configuredTypes.Contains(type))
            .ToArray();
        if (missingTypes.Length > 0)
        {
            problems.Add($"backgrounds: missing types {string.Join(", ", missingTypes)}.");
        }
    }

    private static void ValidateLocations(IReadOnlyList<Location> locations, List<string> problems)
    {
        if (locations.Count == 0)
        {
            problems.Add("locations: catalog is empty.");
            return;
        }

        var configuredIds = new HashSet<LocationId>();
        foreach (var location in locations)
        {
            if (!configuredIds.Add(location.Id))
            {
                problems.Add($"locations: duplicate id '{location.Id.Value}'.");
            }

            if (string.IsNullOrWhiteSpace(location.Name))
            {
                problems.Add($"locations: '{location.Id.Value}' has no name.");
            }

            if (location.TravelTimeMinutes < 0)
            {
                problems.Add($"locations: '{location.Id.Value}' has negative travel time ({location.TravelTimeMinutes}).");
            }

            if (location.ClinicVisitBaseCost < 0)
            {
                problems.Add($"locations: '{location.Id.Value}' has negative clinic visit cost ({location.ClinicVisitBaseCost}).");
            }
        }

        var declaredIds = LocationId.All;
        var missingIds = declaredIds.Where(id => !configuredIds.Contains(id)).ToArray();
        if (missingIds.Length > 0)
        {
            problems.Add($"locations: missing locations {string.Join(", ", missingIds.Select(static id => id.Value))}.");
        }

        var undeclaredIds = configuredIds.Where(id => !declaredIds.Contains(id)).ToArray();
        if (undeclaredIds.Length > 0)
        {
            problems.Add($"locations: unknown location ids {string.Join(", ", undeclaredIds.Select(static id => id.Value))}.");
        }

        var configuredDistricts = locations.Select(static location => location.District).ToHashSet();
        var missingDistricts = Enum.GetValues<DistrictId>()
            .Where(district => !configuredDistricts.Contains(district))
            .ToArray();
        if (missingDistricts.Length > 0)
        {
            problems.Add($"locations: no location covers districts {string.Join(", ", missingDistricts)}.");
        }
    }

    private static void ValidateJobs(IReadOnlyList<JobShift> jobs, List<string> problems)
    {
        if (jobs.Count == 0)
        {
            problems.Add("jobs: catalog is empty.");
            return;
        }

        var configuredTypes = new HashSet<JobType>();
        foreach (var job in jobs)
        {
            if (!configuredTypes.Add(job.Type))
            {
                problems.Add($"jobs: duplicate type {job.Type}.");
            }

            if (string.IsNullOrWhiteSpace(job.Name))
            {
                problems.Add($"jobs: {job.Type} has no name.");
            }

            if (job.BasePay < 0)
            {
                problems.Add($"jobs: {job.Type} has negative base pay ({job.BasePay}).");
            }

            if (job.EnergyCost < 0 || job.StressCost < 0 || job.DurationMinutes <= 0)
            {
                problems.Add($"jobs: {job.Type} has invalid cost/duration values (energy {job.EnergyCost}, stress {job.StressCost}, duration {job.DurationMinutes}).");
            }
        }

        var missingTypes = Enum.GetValues<JobType>()
            .Where(type => !configuredTypes.Contains(type))
            .ToArray();
        if (missingTypes.Length > 0)
        {
            problems.Add($"jobs: missing types {string.Join(", ", missingTypes)}.");
        }
    }

    private static void ValidateRandomEvents(IReadOnlyList<RandomEvent> randomEvents, IReadOnlySet<string> knownInkKnots, List<string> problems)
    {
        if (randomEvents.Count == 0)
        {
            problems.Add("random_events: catalog is empty.");
            return;
        }

        var configuredIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var randomEvent in randomEvents)
        {
            if (!configuredIds.Add(randomEvent.Id))
            {
                problems.Add($"random_events: duplicate id '{randomEvent.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(randomEvent.Description))
            {
                problems.Add($"random_events: '{randomEvent.Id}' has no description.");
            }

            if (randomEvent.MinDay < 0)
            {
                problems.Add($"random_events: '{randomEvent.Id}' has negative min day ({randomEvent.MinDay}).");
            }

            if (randomEvent.Weight <= 0)
            {
                problems.Add($"random_events: '{randomEvent.Id}' has non-positive weight ({randomEvent.Weight}).");
            }

            var inkKnot = randomEvent.Effect.InkKnot;
            if (!string.IsNullOrWhiteSpace(inkKnot) && !knownInkKnots.Contains(inkKnot))
            {
                problems.Add($"random_events: '{randomEvent.Id}' references missing ink knot '{inkKnot}'.");
            }
        }
    }

    private static void ValidateDistrictConditions(IReadOnlyList<DistrictConditionDefinition> districtConditions, List<string> problems)
    {
        if (districtConditions.Count == 0)
        {
            problems.Add("district_conditions: catalog is empty.");
            return;
        }

        var configuredIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var condition in districtConditions)
        {
            if (string.IsNullOrWhiteSpace(condition.Id))
            {
                problems.Add("district_conditions: an entry has no id.");
            }
            else if (!configuredIds.Add(condition.Id))
            {
                problems.Add($"district_conditions: duplicate id '{condition.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(condition.Title))
            {
                problems.Add($"district_conditions: '{condition.Id}' has no title.");
            }

            if (condition.Weight <= 0)
            {
                problems.Add($"district_conditions: '{condition.Id}' has non-positive weight ({condition.Weight}).");
            }

            if (condition.MinDay < 1)
            {
                problems.Add($"district_conditions: '{condition.Id}' has min day below 1 ({condition.MinDay}).");
            }

            if (condition.MinPolicePressure is < 0 or > 100 || condition.MaxPolicePressure is < 0 or > 100)
            {
                problems.Add($"district_conditions: '{condition.Id}' has police pressure bounds outside 0..100.");
            }

            if (condition.MinPolicePressure is int minPressure && condition.MaxPolicePressure is int maxPressure && minPressure > maxPressure)
            {
                problems.Add($"district_conditions: '{condition.Id}' has min police pressure {minPressure} above max {maxPressure}.");
            }
        }
    }

    private static void ValidatePets(IReadOnlyList<PetDefinition> pets, IReadOnlyList<Location> locations, List<string> problems)
    {
        if (pets.Count == 0)
        {
            problems.Add("pets: catalog is empty.");
            return;
        }

        var locationIds = locations.Select(static location => location.Id).ToHashSet();
        var configuredTypes = new HashSet<PetType>();
        foreach (var pet in pets)
        {
            if (!configuredTypes.Add(pet.Type))
            {
                problems.Add($"pets: duplicate type {pet.Type}.");
            }

            if (string.IsNullOrWhiteSpace(pet.Name))
            {
                problems.Add($"pets: {pet.Type} has no name.");
            }

            if (pet.OneTimeCost < 0 || pet.WeeklyCareCost < 0)
            {
                problems.Add($"pets: {pet.Type} has negative cost values.");
            }

            if (pet.MaxOwned <= 0)
            {
                problems.Add($"pets: {pet.Type} has non-positive max owned ({pet.MaxOwned}).");
            }

            if (pet.PurchaseLocationId is LocationId purchaseLocation && !locationIds.Contains(purchaseLocation))
            {
                problems.Add($"pets: {pet.Type} references missing purchase location '{purchaseLocation.Value}'.");
            }
        }

        var missingTypes = Enum.GetValues<PetType>()
            .Where(type => !configuredTypes.Contains(type))
            .ToArray();
        if (missingTypes.Length > 0)
        {
            problems.Add($"pets: missing types {string.Join(", ", missingTypes)}.");
        }
    }

    private static void ValidatePlants(IReadOnlyList<PlantDefinition> plants, IReadOnlyList<Location> locations, List<string> problems)
    {
        if (plants.Count == 0)
        {
            problems.Add("plants: catalog is empty.");
            return;
        }

        var locationIds = locations.Select(static location => location.Id).ToHashSet();
        var configuredTypes = new HashSet<PlantType>();
        foreach (var plant in plants)
        {
            if (!configuredTypes.Add(plant.Type))
            {
                problems.Add($"plants: duplicate type {plant.Type}.");
            }

            if (string.IsNullOrWhiteSpace(plant.Name))
            {
                problems.Add($"plants: {plant.Type} has no name.");
            }

            if (plant.OneTimeCost < 0 || plant.WeeklyCareCost < 0)
            {
                problems.Add($"plants: {plant.Type} has negative cost values.");
            }

            if (plant.HarvestCycleDays < 0 || plant.HarvestSalePrice < 0)
            {
                problems.Add($"plants: {plant.Type} has negative harvest values.");
            }

            if (!locationIds.Contains(plant.PurchaseLocationId))
            {
                problems.Add($"plants: {plant.Type} references missing purchase location '{plant.PurchaseLocationId.Value}'.");
            }
        }

        var missingTypes = Enum.GetValues<PlantType>()
            .Where(type => !configuredTypes.Contains(type))
            .ToArray();
        if (missingTypes.Length > 0)
        {
            problems.Add($"plants: missing types {string.Join(", ", missingTypes)}.");
        }
    }

    private static bool IsPercentage(int value) => value is >= 0 and <= 100;
}
