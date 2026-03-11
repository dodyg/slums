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
        NpcRegistry.GetReachableNpcs(LocationId.Pharmacy, policePressure: 0).Should().Contain(NpcId.PharmacistMariam);
        NpcRegistry.GetReachableNpcs(LocationId.Depot, policePressure: 0).Should().Contain(NpcId.DispatcherSafaa);
        NpcRegistry.GetReachableNpcs(LocationId.Laundry, policePressure: 0).Should().Contain(NpcId.LaundryOwnerIman);
    }

    [Test]
    public void GetConversationKnot_ShouldUseWarmVariants_WhenTrustIsHigh()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 20, 1);
        state.SetNpcRelationship(NpcId.CafeOwnerNadia, 20, 1);
        state.SetNpcRelationship(NpcId.PharmacistMariam, 20, 1);

        NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0).Should().Be("nurse_salma_warm");
        NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0).Should().Be("nadia_cafe_warm");
        NpcRegistry.GetConversationKnot(NpcId.PharmacistMariam, state, policePressure: 0).Should().Be("mariam_pharmacy_warm");
    }

    [Test]
    public void GetConversationKnot_ShouldUseDebtVariant_WhenSalmaIsOwed()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NurseSalma, lastFavorDay: 2, lastRefusalDay: 0, hasUnpaidDebt: true, wasEmbarrassed: false, wasHelped: false, recentContactCount: 1);

        NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0, currentDay: 3, honestShiftsCompleted: 0, crimesCommitted: 0)
            .Should().Be("nurse_salma_debt");
    }

    [Test]
    public void GetConversationKnot_ShouldUseSuspiciousVariant_WhenDoubleLifeIsVisible()
    {
        var state = new RelationshipState();

        NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0, currentDay: 5, honestShiftsCompleted: 4, crimesCommitted: 2)
            .Should().Be("nurse_salma_suspicious");
        NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0, currentDay: 5, honestShiftsCompleted: 4, crimesCommitted: 2)
            .Should().Be("nadia_cafe_double_life");
    }

    [Test]
    public void GetConversationKnot_ShouldUseHelpAndEmbarrassmentVariants_WhenMemoryFlagsExist()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NeighborMona, 0, 0, hasUnpaidDebt: false, wasEmbarrassed: false, wasHelped: true, recentContactCount: 1);
        state.SetNpcRelationshipMemory(NpcId.WorkshopBossAbuSamir, 0, 0, hasUnpaidDebt: false, wasEmbarrassed: true, wasHelped: false, recentContactCount: 1);

        NpcRegistry.GetConversationKnot(NpcId.NeighborMona, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0)
            .Should().Be("neighbor_mona_helped");
        NpcRegistry.GetConversationKnot(NpcId.WorkshopBossAbuSamir, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0)
            .Should().Be("abu_samir_embarrassed");
    }
}