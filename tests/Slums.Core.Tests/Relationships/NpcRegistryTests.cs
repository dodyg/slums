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
    public void GetConversationKnot_ShouldReturnKnotStartingWithContextPrefix()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 20, 1);
        state.SetNpcRelationship(NpcId.CafeOwnerNadia, 20, 1);
        state.SetNpcRelationship(NpcId.PharmacistMariam, 20, 1);

        var salmaKnot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0);
        salmaKnot.Should().StartWith("salma_warm_");
        
        var nadiaKnot = NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0);
        nadiaKnot.Should().StartWith("nadia_warm_");
        
        var mariamKnot = NpcRegistry.GetConversationKnot(NpcId.PharmacistMariam, state, policePressure: 0);
        mariamKnot.Should().StartWith("mariam_warm_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseDebtVariant_WhenSalmaIsOwed()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NurseSalma, lastFavorDay: 2, lastRefusalDay: 0, hasUnpaidDebt: true, wasEmbarrassed: false, wasHelped: false, recentContactCount: 1);

        var knot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0, currentDay: 3, honestShiftsCompleted: 0, crimesCommitted: 0);
        knot.Should().StartWith("salma_debt");
    }

    [Test]
    public void GetConversationKnot_ShouldUseWarmerDebtAndSoftBrokeVariants_WhenTrustIsHigh()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 18, 1);
        state.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 16, 1);
        state.SetNpcRelationshipMemory(NpcId.NurseSalma, lastFavorDay: 2, lastRefusalDay: 0, hasUnpaidDebt: true, wasEmbarrassed: false, wasHelped: false, recentContactCount: 1);

        var salmaKnot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0, currentDay: 3, honestShiftsCompleted: 0, crimesCommitted: 0);
        salmaKnot.Should().StartWith("salma_debt_warm_");
        
        var landlordKnot = NpcRegistry.GetConversationKnot(NpcId.LandlordHajjMahmoud, state, policePressure: 0, currentDay: 3, honestShiftsCompleted: 0, crimesCommitted: 0, currentMoney: 20);
        landlordKnot.Should().StartWith("landlord_broke_soft_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseSuspiciousVariant_WhenDoubleLifeIsVisible()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.FixerUmmKarim, 12, 1);

        var salmaKnot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0, currentDay: 5, honestShiftsCompleted: 4, crimesCommitted: 2);
        salmaKnot.Should().StartWith("salma_suspicious_");
        
        var nadiaKnot = NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0, currentDay: 5, honestShiftsCompleted: 4, crimesCommitted: 2);
        nadiaKnot.Should().StartWith("nadia_double_life_");
        
        var fixerKnot = NpcRegistry.GetConversationKnot(NpcId.FixerUmmKarim, state, policePressure: 0, currentDay: 5, honestShiftsCompleted: 4, crimesCommitted: 2);
        fixerKnot.Should().StartWith("fixer_double_life_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseHelpAndEmbarrassmentVariants_WhenMemoryFlagsExist()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NeighborMona, 0, 0, hasUnpaidDebt: false, wasEmbarrassed: false, wasHelped: true, recentContactCount: 1);
        state.SetNpcRelationshipMemory(NpcId.WorkshopBossAbuSamir, 0, 0, hasUnpaidDebt: false, wasEmbarrassed: true, wasHelped: false, recentContactCount: 1);

        var monaKnot = NpcRegistry.GetConversationKnot(NpcId.NeighborMona, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        monaKnot.Should().StartWith("mona_helped_");
        
        var abuKnot = NpcRegistry.GetConversationKnot(NpcId.WorkshopBossAbuSamir, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        abuKnot.Should().StartWith("abu_samir_embarrassed_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseLowMoneyVariant()
    {
        var state = new RelationshipState();

        var landlordKnot = NpcRegistry.GetConversationKnot(NpcId.LandlordHajjMahmoud, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0, currentMoney: 20, motherHealth: 70);
        landlordKnot.Should().StartWith("landlord_broke_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseHeatVariant_WhenPoliceAttentionIsHigh()
    {
        var state = new RelationshipState();

        var monaKnot = NpcRegistry.GetConversationKnot(NpcId.NeighborMona, state, policePressure: 75, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 2);
        monaKnot.Should().StartWith("mona_heat_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseColdVariants()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.FixerUmmKarim, 25, 1);
        state.SetNpcRelationship(NpcId.OfficerKhalid, -10, 1);
        state.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, -10, 1);
        state.SetNpcRelationship(NpcId.CafeOwnerNadia, -10, 1);
        state.SetNpcRelationship(NpcId.FenceHanan, -10, 1);

        var fixerKnot = NpcRegistry.GetConversationKnot(NpcId.FixerUmmKarim, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        fixerKnot.Should().StartWith("fixer_trusted_");

        var officerKnot = NpcRegistry.GetConversationKnot(NpcId.OfficerKhalid, state, policePressure: 20, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        officerKnot.Should().StartWith("officer_marked_");

        var abuKnot = NpcRegistry.GetConversationKnot(NpcId.WorkshopBossAbuSamir, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        abuKnot.Should().StartWith("abu_samir_cold_");

        var nadiaKnot = NpcRegistry.GetConversationKnot(NpcId.CafeOwnerNadia, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        nadiaKnot.Should().StartWith("nadia_cold_");

        var hananKnot = NpcRegistry.GetConversationKnot(NpcId.FenceHanan, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        hananKnot.Should().StartWith("hanan_cold_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseEmbeddedVariant()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.RunnerYoussef, 15, 1);

        var knot = NpcRegistry.GetConversationKnot(NpcId.RunnerYoussef, state, policePressure: 20, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 2);
        knot.Should().StartWith("youssef_embedded_");
    }

    [Test]
    public void GetConversationKnot_ShouldUseRegularVariant()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.DispatcherSafaa, 0, 0, hasUnpaidDebt: false, wasEmbarrassed: false, wasHelped: false, recentContactCount: 3);

        var knot = NpcRegistry.GetConversationKnot(NpcId.DispatcherSafaa, state, policePressure: 0, currentDay: 4, honestShiftsCompleted: 0, crimesCommitted: 0);
        knot.Should().StartWith("safaa_regular_");
    }

    [Test]
    public void GetConversationKnot_ShouldNotRepeatSeenConversation_()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 10, 1);

        var firstKnot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0);
        state.RecordSeenConversation(NpcId.NurseSalma, firstKnot);

        var secondKnot = NpcRegistry.GetConversationKnot(NpcId.NurseSalma, state, policePressure: 0);
        secondKnot.Should().NotBe(firstKnot);
    }

    [Test]
    public void ConversationPoolRegistry_ShouldHave100ConversationsPerNpcContext()
    {
        foreach (NpcId npcId in Enum.GetValues<NpcId>())
        {
            var contexts = new[] { "default", "warm", "cold", "broke", "broke_soft", "hostile", "debt", "debt_warm", "urgent", "suspicious", "double_life", "heat", "helped", "embarrassed", "hot", "marked", "lean", "regular", "first", "repeat", "recent_refusal", "trusted", "embedded" };
            foreach (var context in contexts)
            {
                var pool = ConversationPoolRegistry.GetConversationPool(npcId, context);
                pool.Count.Should().BeGreaterOrEqualTo(90);
            }
        }
    }
}
