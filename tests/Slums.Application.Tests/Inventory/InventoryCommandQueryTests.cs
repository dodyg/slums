using FluentAssertions;
using Slums.Application.Inventory;
using Slums.Core.Inventory;
using Slums.Core.State;
using Slums.Core.World.News;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Application.Tests.Inventory;

/// <summary>Dedicated coverage for the inventory command/query pair.</summary>
internal sealed class InventoryCommandQueryTests
{
    [Test]
    public void AcquireItem_ShouldAddQuantityUpToTheConfiguredMaximum()
    {
        var catalog = TestContent.CatalogWith(items: [MakeItem("ration_ticket", max: 3)]);
        var session = TestSessions.Create(contentCatalog: catalog);

        var first = new AcquireItemCommand().Execute(session, "ration_ticket", 2);
        var second = new AcquireItemCommand().Execute(session, "ration_ticket", 2);

        first.Success.Should().BeTrue();
        second.Success.Should().BeFalse();
        session.Inventory.GetQuantity("ration_ticket").Should().Be(2);
    }

    [Test]
    public void AcquireItem_ShouldRejectUnknownItems()
    {
        var session = TestSessions.Create();

        var result = new AcquireItemCommand().Execute(session, "not_an_item");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not part of the current catalog");
    }

    [Test]
    public void InventoryMenu_ShouldDescribeKnownItemsAndFallBackForUnknownIds()
    {
        var catalog = TestContent.CatalogWith(items: [MakeItem("known_item", max: 5, name: "Known Item", description: "A known thing.")]);
        var session = TestSessions.Create(contentCatalog: catalog);
        session.Inventory.Add("known_item", 1, 5);

        var entries = new InventoryMenuQuery().GetEntries(InventoryMenuContext.Create(session));

        entries.Should().ContainSingle();
        entries[0].Id.Should().Be("known_item");
        entries[0].Name.Should().Be("Known Item");
        entries[0].Description.Should().Be("A known thing.");
    }

    private static ItemDefinition MakeItem(string id, int max, string? name = null, string? description = null)
    {
        return new ItemDefinition { Id = id, Name = name ?? id, Description = description ?? id, MaximumQuantity = max };
    }
}
