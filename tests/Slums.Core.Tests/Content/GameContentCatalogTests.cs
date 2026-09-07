using FluentAssertions;
using Slums.Core.Content;
using Slums.Core.Events;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.Content;

internal sealed class GameContentCatalogTests
{
    [Test]
    public void Constructor_ShouldSnapshotAndHideMutableInputCollections()
    {
        var events = new List<RandomEvent>
        {
            new("event", "Description", new RandomEventEffect(), 1, 1, null)
        };
        var conditions = new List<DistrictConditionDefinition>
        {
            new() { Id = "condition", District = DistrictId.Imbaba, Title = "Condition", Weight = 1, MinDay = 1 }
        };
        var schedules = new List<NpcScheduleDefinition>();
        var catalog = new GameContentCatalog(events, conditions, schedules);

        events.Clear();
        conditions.Clear();

        catalog.RandomEvents.Should().ContainSingle();
        catalog.DistrictConditions.Should().ContainSingle();
        catalog.RandomEvents.Should().NotBeAssignableTo<RandomEvent[]>();
        catalog.DistrictConditions.Should().NotBeAssignableTo<DistrictConditionDefinition[]>();
    }
}
