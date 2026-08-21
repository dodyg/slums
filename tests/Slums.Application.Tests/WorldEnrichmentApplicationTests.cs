using Slums.Application.Inventory;
using Slums.Application.News;
using Slums.Core.Inventory;
using Slums.Core.World.News;
using TUnit.Core;

namespace Slums.Application.Tests;

[NotInParallel]
internal sealed class WorldEnrichmentApplicationTests
{
    [Test]
    public async Task NewsMenuQuery_ShouldShowVisibleCostsAndUnavailableRequirements()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "application_news",
            Headline = "A useful headline",
            Body = "A useful body",
            SourceLabel = "A source",
            DurationDays = 3,
            Responses = [new NewsResponseDefinition
            {
                Id = "prepare",
                Label = "Prepare",
                Type = NewsResponseType.Prepare,
                MoneyCost = 20,
                RequiredItemId = "work_papers",
                RequiredItemQuantity = 1
            }]
        };
        NewsRegistry.Configure([definition]);
        ItemRegistry.Configure([new ItemDefinition { Id = "work_papers", Name = "Work papers", Description = "papers", MaximumQuantity = 1 }]);
        var gameSession = new Slums.Core.State.GameSession();
        gameSession.News.Activate(definition, gameSession.Clock.Day);

        var status = new NewsMenuQuery().GetStatus(NewsMenuContext.Create(gameSession), gameSession.Inventory.Quantities, gameSession.Player.Stats.Money);

        await Assert.That(status.Flashes.Single().Responses.Single().IsAvailable).IsFalse();
        await Assert.That(status.Flashes.Single().Responses.Single().CostSummary).Contains("20 LE");
        await Assert.That(status.Flashes.Single().Responses.Single().DisabledReason).Contains("work_papers");
    }

    [Test]
    public async Task NewsResponseCommand_ShouldConsumeItemAndMarkResponseUsed()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "response_news",
            Headline = "A response headline",
            Body = "A response body",
            SourceLabel = "A source",
            DurationDays = 3,
            Responses = [new NewsResponseDefinition
            {
                Id = "share",
                Label = "Share",
                Type = NewsResponseType.ShareInformation,
                RequiredItemId = "work_papers",
                RequiredItemQuantity = 1,
                OutcomeMessage = "Shared locally."
            }]
        };
        NewsRegistry.Configure([definition]);
        var gameSession = new Slums.Core.State.GameSession();
        gameSession.News.Activate(definition, gameSession.Clock.Day);
        gameSession.Inventory.Add("work_papers", 1, 1);

        var result = new NewsResponseCommand().Execute(gameSession, definition.Id, "share");

        await Assert.That(result.Success).IsTrue();
        await Assert.That(gameSession.Inventory.GetQuantity("work_papers")).IsEqualTo(0);
        await Assert.That(gameSession.ActiveNews.Single().UsedResponseId).IsEqualTo("share");
        await Assert.That(new NewsResponseCommand().Execute(gameSession, definition.Id, "share").Success).IsFalse();
    }

    [Test]
    public async Task InventoryMenuQuery_ShouldDescribeKnownItems()
    {
        ItemRegistry.Configure([new ItemDefinition { Id = "repair_component", Name = "Repair component", Description = "A connector", MaximumQuantity = 2 }]);
        var gameSession = new Slums.Core.State.GameSession();
        gameSession.Inventory.Add("repair_component", 1, 2);

        var entries = new InventoryMenuQuery().GetEntries(InventoryMenuContext.Create(gameSession));

        await Assert.That(entries).Count().IsEqualTo(1);
        await Assert.That(entries.Single().Name).IsEqualTo("Repair component");
    }
}
