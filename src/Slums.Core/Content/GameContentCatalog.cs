using Slums.Core.Events;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Content;

/// <summary>
/// Immutable world-definition snapshot owned by a game session.
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
        IEnumerable<NpcScheduleDefinition> npcSchedules)
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

    /// <summary>Captures already configured content adapters for compatibility bootstrapping.</summary>
    public static GameContentCatalog FromConfiguredRegistries()
    {
        return new GameContentCatalog(
            BackgroundRegistry.AllBackgrounds,
            WorldState.AllLocations,
            JobRegistry.AllJobs,
            RandomEventRegistry.AllEvents,
            DistrictConditionRegistry.AllDefinitions,
            NpcScheduleRegistry.All);
    }
}
