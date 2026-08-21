using Slums.Core.Inventory;
using Slums.Core.Randomness;
using Slums.Core.World;
using Slums.Core.World.News;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

[NotInParallel]
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

    [Test]
    public async Task Snapshot_ShouldPreserveTheFutureSeededNewsSequence()
    {
        var definitions = new[]
        {
            new NewsFlashDefinition { Id = "future_one", Headline = "One", Body = "One", SourceLabel = "Source", MinimumDay = 1, Weight = 1, DurationDays = 2 },
            new NewsFlashDefinition { Id = "future_two", Headline = "Two", Body = "Two", SourceLabel = "Source", MinimumDay = 1, Weight = 1, DurationDays = 2 }
        };
        NewsRegistry.Configure(definitions);
        var original = new Slums.Core.State.GameSession(new GameRandom(9988));
        var restored = GameSessionSnapshot.Capture(original).Restore();

        for (var day = 2; day <= 20; day++)
        {
            var originalResult = NewsService.ResolveStartOfDay(original.News, original.Infrastructure, original.EventJournal, day, original.SharedRandom)?.Id;
            var restoredResult = NewsService.ResolveStartOfDay(restored.News, restored.Infrastructure, restored.EventJournal, day, restored.SharedRandom)?.Id;
            await Assert.That(restoredResult).IsEqualTo(originalResult);
            await Assert.That(restored.ActiveNews.Select(static news => news.DefinitionId)).IsEquivalentTo(original.ActiveNews.Select(static news => news.DefinitionId));
        }
    }
}
