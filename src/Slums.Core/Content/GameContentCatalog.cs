using Slums.Core.Events;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Inventory;
using Slums.Core.Robotics;
using Slums.Core.Technology;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Core.Content;

/// <summary>
/// Immutable world-definition snapshot owned by a game session. Sessions never mutate the
/// catalog, so instances can be reused freely.
/// </summary>
public sealed class GameContentCatalog
{
    /// <summary>Creates a catalog from already validated content definitions.</summary>
    public GameContentCatalog(
        IEnumerable<Background> backgrounds,
        IEnumerable<Location> locations,
        IEnumerable<JobShift> jobs,
        IEnumerable<RandomEvent> randomEvents,
        IEnumerable<DistrictConditionDefinition> districtConditions,
        IEnumerable<NpcScheduleDefinition> npcSchedules,
        IEnumerable<PetDefinition>? pets = null,
        IEnumerable<PlantDefinition>? plants = null,
        IEnumerable<RobotDefinition>? robots = null,
        IEnumerable<NewsFlashDefinition>? newsFlashes = null,
        IEnumerable<ItemDefinition>? items = null,
        IEnumerable<InvestmentDefinition>? investments = null,
        IEnumerable<DigitalServiceActionDefinition>? digitalServices = null,
        IEnumerable<TechnicalRepairActionDefinition>? technicalRepairs = null)
    {
        ArgumentNullException.ThrowIfNull(backgrounds);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(jobs);
        ArgumentNullException.ThrowIfNull(randomEvents);
        ArgumentNullException.ThrowIfNull(districtConditions);
        ArgumentNullException.ThrowIfNull(npcSchedules);

        Backgrounds = Array.AsReadOnly(backgrounds.ToArray());
        Locations = Array.AsReadOnly(locations.ToArray());
        Jobs = Array.AsReadOnly(jobs.ToArray());
        RandomEvents = Array.AsReadOnly(randomEvents.ToArray());
        DistrictConditions = Array.AsReadOnly(districtConditions.ToArray());
        NpcSchedules = Array.AsReadOnly(npcSchedules.ToArray());
        Pets = Array.AsReadOnly((pets ?? []).ToArray());
        Plants = Array.AsReadOnly((plants ?? []).ToArray());
        Robots = Array.AsReadOnly((robots ?? []).ToArray());
        NewsFlashes = Array.AsReadOnly((newsFlashes ?? []).ToArray());
        Items = Array.AsReadOnly((items ?? []).ToArray());
        Investments = Array.AsReadOnly((investments ?? []).ToArray());
        DigitalServices = Array.AsReadOnly((digitalServices ?? []).ToArray());
        TechnicalRepairs = Array.AsReadOnly((technicalRepairs ?? []).ToArray());
    }

    /// <summary>Background definitions available to this session.</summary>
    public IReadOnlyList<Background> Backgrounds { get; }

    /// <summary>Location definitions available to this session.</summary>
    public IReadOnlyList<Location> Locations { get; }

    /// <summary>Job definitions available to this session.</summary>
    public IReadOnlyList<JobShift> Jobs { get; }

    /// <summary>Random events available to this session.</summary>
    public IReadOnlyList<RandomEvent> RandomEvents { get; }

    /// <summary>District condition definitions available to this session.</summary>
    public IReadOnlyList<DistrictConditionDefinition> DistrictConditions { get; }

    /// <summary>NPC schedules available to this session.</summary>
    public IReadOnlyList<NpcScheduleDefinition> NpcSchedules { get; }

    /// <summary>Pet definitions available to this session.</summary>
    public IReadOnlyList<PetDefinition> Pets { get; }

    /// <summary>Plant definitions available to this session.</summary>
    public IReadOnlyList<PlantDefinition> Plants { get; }

    /// <summary>Robot definitions available to this session.</summary>
    public IReadOnlyList<RobotDefinition> Robots { get; }

    /// <summary>News definitions available to this session.</summary>
    public IReadOnlyList<NewsFlashDefinition> NewsFlashes { get; }

    /// <summary>Inventory item definitions available to this session.</summary>
    public IReadOnlyList<ItemDefinition> Items { get; }

    /// <summary>Investment definitions available to this session.</summary>
    public IReadOnlyList<InvestmentDefinition> Investments { get; }

    /// <summary>Digital service action definitions available to this session.</summary>
    public IReadOnlyList<DigitalServiceActionDefinition> DigitalServices { get; }

    /// <summary>Technical repair action definitions available to this session.</summary>
    public IReadOnlyList<TechnicalRepairActionDefinition> TechnicalRepairs { get; }

    /// <summary>Gets the job shift definition for <paramref name="type"/>, failing fast when missing.</summary>
    public JobShift GetJob(JobType type)
    {
        return Jobs.FirstOrDefault(job => job.Type == type)
            ?? throw new InvalidOperationException($"No job definition found for {type}.");
    }

    /// <summary>Gets the background definition for <paramref name="type"/>, failing fast when missing.</summary>
    public Background GetBackground(BackgroundType type)
    {
        return Backgrounds.FirstOrDefault(background => background.Type == type)
            ?? throw new InvalidOperationException($"No background definition found for {type}.");
    }

    /// <summary>Gets the investment definition for <paramref name="type"/>, or <c>null</c> when unknown.</summary>
    public InvestmentDefinition? GetInvestment(InvestmentType type)
    {
        return Investments.FirstOrDefault(definition => definition.Type == type);
    }

    /// <summary>Gets the pet definition for <paramref name="type"/>, failing fast when missing.</summary>
    public PetDefinition GetPet(PetType type)
    {
        return Pets.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No pet definition found for {type}.");
    }

    /// <summary>Gets the plant definition for <paramref name="type"/>, failing fast when missing.</summary>
    public PlantDefinition GetPlant(PlantType type)
    {
        return Plants.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No plant definition found for {type}.");
    }

    /// <summary>Gets the robot definition for <paramref name="type"/>, failing fast when missing.</summary>
    public RobotDefinition GetRobot(RobotType type)
    {
        return Robots.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No robot definition found for {type}.");
    }

    /// <summary>Gets the item definition for <paramref name="id"/>, or <c>null</c> when unknown.</summary>
    public ItemDefinition? GetItem(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Items.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>Gets the news flash definition for <paramref name="id"/>, or <c>null</c> when unknown.</summary>
    public NewsFlashDefinition? GetNewsFlashById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return NewsFlashes.FirstOrDefault(definition => definition.Id == id);
    }

    /// <summary>Gets the district condition definition for <paramref name="conditionId"/>, or <c>null</c> when unknown.</summary>
    public DistrictConditionDefinition? GetDistrictConditionById(string? conditionId)
    {
        if (string.IsNullOrWhiteSpace(conditionId))
        {
            return null;
        }

        return DistrictConditions.FirstOrDefault(definition => definition.Id == conditionId);
    }

    /// <summary>Gets the digital service action definition for <paramref name="actionType"/>, failing fast when missing.</summary>
    public DigitalServiceActionDefinition GetDigitalService(DigitalServiceActionType actionType)
    {
        return DigitalServices.FirstOrDefault(definition => definition.Type == actionType)
            ?? throw new InvalidOperationException($"No digital service definition found for {actionType}.");
    }

    /// <summary>Gets the technical repair action definition for <paramref name="actionType"/>, failing fast when missing.</summary>
    public TechnicalRepairActionDefinition GetTechnicalRepair(TechnicalRepairActionType actionType)
    {
        return TechnicalRepairs.FirstOrDefault(definition => definition.Type == actionType)
            ?? throw new InvalidOperationException($"No technical repair definition found for {actionType}.");
    }
}
