using FluentAssertions;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Relationships;

internal sealed class NpcRegistryTests
{
    [Test]
    public void GetReachableNpcs_ShouldIncludeNeighborAtHome()
    {
        var npcs = NpcRegistry.GetReachableNpcs(LocationId.Home, policePressure: 0);

        npcs.Should().Contain(NpcId.NeighborMona);
        npcs.Should().Contain(NpcId.LandlordHajjMahmoud);
    }

    [Test]
    public void GetReachableNpcs_ShouldIncludeLocationSpecificNpcs()
    {
        NpcRegistry.GetReachableNpcs(LocationId.Clinic, policePressure: 0).Should().Contain(NpcId.NurseSalma);
        NpcRegistry.GetReachableNpcs(LocationId.Workshop, policePressure: 0).Should().Contain(NpcId.WorkshopBossAbuSamir);
        NpcRegistry.GetReachableNpcs(LocationId.Cafe, policePressure: 0).Should().Contain(NpcId.CafeOwnerNadia);
        NpcRegistry.GetReachableNpcs(LocationId.Market, policePressure: 0).Should().Contain(NpcId.FenceHanan);
        NpcRegistry.GetReachableNpcs(LocationId.Square, policePressure: 0).Should().Contain(NpcId.RunnerYoussef);
    }

    [Test]
    public void GetConversationKnot_ShouldUseWarmVariants_WhenTrustIsHigh()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 20, 1);
        state.SetNpcRelationship(NpcId.CafeOwnerNadia, 20, 1);

        NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0).Should().Be("nurse_salma_warm");
        NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0).Should().Be("nadia_cafe_warm");
    }
}