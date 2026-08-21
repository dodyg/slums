using Slums.Core.Inventory;
using Slums.Core.World;
using Slums.Core.World.News;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class WorldEnrichmentSnapshotTests
{
    [Test]
    public async Task Snapshot_ShouldRoundTripNewsInfrastructureAndInventory()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "snapshot_news",
            Headline = "Snapshot headline",
            Body = "Snapshot body",
            SourceLabel = "Snapshot source",
            DurationDays = 4
        };
        NewsRegistry.Configure([definition]);

        var original = new Slums.Core.State.GameSession();
        original.News.Activate(definition, 3);
        original.Infrastructure.StartDisruption(DistrictId.Dokki, InfrastructureServiceType.Transport, InfrastructureSeverity.Strained, 3, 3, definition.Id);
        original.Inventory.Add("transit_pass", 2, 5);

        var restored = GameSessionSnapshot.Capture(original).Restore();

        await Assert.That(restored.ActiveNews.Single().DefinitionId).IsEqualTo("snapshot_news");
        await Assert.That(restored.Infrastructure.Get(DistrictId.Dokki, InfrastructureServiceType.Transport).RemainingDays).IsEqualTo(3);
        await Assert.That(restored.Inventory.GetQuantity("transit_pass")).IsEqualTo(2);
    }
}
