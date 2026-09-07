using Slums.Core.Events;
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
        IEnumerable<RandomEvent> randomEvents,
        IEnumerable<DistrictConditionDefinition> districtConditions,
        IEnumerable<NpcScheduleDefinition> npcSchedules)
    {
        ArgumentNullException.ThrowIfNull(randomEvents);
        ArgumentNullException.ThrowIfNull(districtConditions);
        ArgumentNullException.ThrowIfNull(npcSchedules);

        RandomEvents = Array.AsReadOnly(randomEvents.ToArray());
        DistrictConditions = Array.AsReadOnly(districtConditions.ToArray());
        NpcSchedules = Array.AsReadOnly(npcSchedules.ToArray());
    }

    /// <summary>Random events available to this session.</summary>
    public IReadOnlyList<RandomEvent> RandomEvents { get; }

    /// <summary>District condition definitions available to this session.</summary>
    public IReadOnlyList<DistrictConditionDefinition> DistrictConditions { get; }

    /// <summary>NPC schedules available to this session.</summary>
    public IReadOnlyList<NpcScheduleDefinition> NpcSchedules { get; }

    /// <summary>Captures the legacy registry configuration for compatibility bootstrapping.</summary>
    public static GameContentCatalog FromLegacyRegistries()
    {
        return new GameContentCatalog(
            RandomEventRegistry.AllEvents,
            DistrictConditionRegistry.AllDefinitions,
            NpcScheduleRegistry.All);
    }
}
