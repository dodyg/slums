using FluentAssertions;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Relationships;

internal sealed class VendorTarekTests
{
    [Test]
    public void GetName_ShouldReturnTarek()
    {
        NpcRegistry.GetName(NpcId.VendorTarek).Should().Be("Tarek");
    }

    [Test]
    public void GetReachableNpcs_ShouldIncludeTarek_AtSquare()
    {
        var npcs = NpcRegistry.GetReachableNpcs(LocationId.Square, policePressure: 0);

        npcs.Should().Contain(NpcId.VendorTarek);
    }

    [Test]
    public void GetReachableNpcs_ShouldNotIncludeTarek_AtHome()
    {
        var npcs = NpcRegistry.GetReachableNpcs(LocationId.Home, policePressure: 0);

        npcs.Should().NotContain(NpcId.VendorTarek);
    }

    [Test]
    public void GetConversationKnot_ShouldReturnValidKnot_ForTarek()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.VendorTarek, 5, 1);

        var knot = NpcRegistry.GetConversationKnot(NpcId.VendorTarek, relationships, policePressure: 10);

        knot.Should().StartWith("tarek_");
    }

    [Test]
    public void ConversationPoolPrefixes_ShouldHaveAllTarekPrefixes()
    {
        ConversationPoolPrefixes.TarekDefault.Should().Be("tarek_default");
        ConversationPoolPrefixes.TarekWarm.Should().Be("tarek_warm");
        ConversationPoolPrefixes.TarekStreetwise.Should().Be("tarek_streetwise");
    }
}
