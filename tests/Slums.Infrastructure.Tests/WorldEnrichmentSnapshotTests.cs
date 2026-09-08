using Slums.Core.Inventory;
using Slums.Core.Randomness;
using Slums.Core.World;
using Slums.Core.World.News;
using Slums.Infrastructure.Persistence;
using Slums.TestSupport;
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
        var catalog = TestContent.CatalogWith(
            newsFlashes: [definition],
            items: [new ItemDefinition
            {
                Id = "transit_pass",
                Name = "Transit pass",
                Description = "A route token",
                MaximumQuantity = 5
            }]);
        var original = TestSessions.Create(contentCatalog: catalog);
        original.News.Activate(definition, 3);
        original.Infrastructure.StartDisruption(DistrictId.Dokki, InfrastructureServiceType.Transport, InfrastructureSeverity.Strained, 3, 3, definition.Id);
        original.Inventory.Add("transit_pass", 2, 5);

        var restored = GameSessionSnapshot.Capture(original).Restore(catalog);

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
        var catalog = TestContent.CatalogWith(newsFlashes: definitions);
        var original = TestSessions.Create(new GameRandom(9988), catalog);
        var restored = GameSessionSnapshot.Capture(original).Restore(catalog);

        for (var day = 2; day <= 20; day++)
        {
            var originalResult = NewsService.ResolveStartOfDay(original.News, original.Infrastructure, original.EventJournal, day, original.SharedRandom, original.ContentCatalog.NewsFlashes)?.Id;
            var restoredResult = NewsService.ResolveStartOfDay(restored.News, restored.Infrastructure, restored.EventJournal, day, restored.SharedRandom, restored.ContentCatalog.NewsFlashes)?.Id;
            await Assert.That(restoredResult).IsEqualTo(originalResult);
            await Assert.That(restored.ActiveNews.Select(static news => news.DefinitionId)).IsEquivalentTo(original.ActiveNews.Select(static news => news.DefinitionId));
        }
    }
}
