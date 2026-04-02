using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Relationships;

internal sealed class RelationshipStateTests
{
    [Test]
    public async Task ModifyNpcTrust_ClampsToUpperBound()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NeighborMona, 95, 0);
        state.ModifyNpcTrust(NpcId.NeighborMona, 20);
        var rel = state.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsEqualTo(100);
    }

    [Test]
    public async Task ModifyNpcTrust_ClampsToLowerBound()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NeighborMona, -90, 0);
        state.ModifyNpcTrust(NpcId.NeighborMona, -50);
        var rel = state.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsEqualTo(-100);
    }

    [Test]
    public async Task ModifyNpcTrust_AppliesSmallDelta()
    {
        var state = new RelationshipState();
        state.ModifyNpcTrust(NpcId.NeighborMona, 5);
        await Assert.That(state.GetNpcRelationship(NpcId.NeighborMona).Trust).IsEqualTo(5);
    }

    [Test]
    public async Task ModifyNpcTrust_NegativeDelta()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.FixerUmmKarim, 30, 0);
        state.ModifyNpcTrust(NpcId.FixerUmmKarim, -15);
        await Assert.That(state.GetNpcRelationship(NpcId.FixerUmmKarim).Trust).IsEqualTo(15);
    }

    [Test]
    public async Task ModifyNpcTrust_StaysAtExactBoundary()
    {
        var state = new RelationshipState();
        state.SetNpcRelationship(NpcId.NurseSalma, 100, 0);
        state.ModifyNpcTrust(NpcId.NurseSalma, 0);
        await Assert.That(state.GetNpcRelationship(NpcId.NurseSalma).Trust).IsEqualTo(100);
    }

    [Test]
    public async Task SetFactionStanding_SetsReputation()
    {
        var state = new RelationshipState();
        state.SetFactionStanding(FactionId.ImbabaCrew, 50);
        await Assert.That(state.GetFactionStanding(FactionId.ImbabaCrew).Reputation).IsEqualTo(50);
    }

    [Test]
    public async Task GetFactionStanding_ReturnsZeroForUnset()
    {
        var state = new RelationshipState();
        await Assert.That(state.GetFactionStanding(FactionId.DokkiThugs).Reputation).IsEqualTo(0);
    }

    [Test]
    public async Task RecordFavor_SetsLastFavorDayAndHelpedAndContact()
    {
        var state = new RelationshipState();
        state.RecordFavor(NpcId.NeighborMona, 5);
        var rel = state.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.LastFavorDay).IsEqualTo(5);
        await Assert.That(rel.WasHelped).IsTrue();
        await Assert.That(rel.RecentContactCount).IsEqualTo(1);
    }

    [Test]
    public async Task RecordFavor_WithDebt_SetsDebtFlag()
    {
        var state = new RelationshipState();
        state.RecordFavor(NpcId.NeighborMona, 5, hasUnpaidDebt: true);
        await Assert.That(state.GetNpcRelationship(NpcId.NeighborMona).HasUnpaidDebt).IsTrue();
    }

    [Test]
    public async Task RecordRefusal_SetsLastRefusalDayAndContact()
    {
        var state = new RelationshipState();
        state.RecordRefusal(NpcId.FixerUmmKarim, 10);
        var rel = state.GetNpcRelationship(NpcId.FixerUmmKarim);
        await Assert.That(rel.LastRefusalDay).IsEqualTo(10);
        await Assert.That(rel.RecentContactCount).IsEqualTo(1);
    }

    [Test]
    public async Task RecordContact_UpdatesLastSeenDayAndContactCount()
    {
        var state = new RelationshipState();
        state.RecordContact(NpcId.NurseSalma, 7);
        state.RecordContact(NpcId.NurseSalma, 12);
        var rel = state.GetNpcRelationship(NpcId.NurseSalma);
        await Assert.That(rel.LastSeenDay).IsEqualTo(12);
        await Assert.That(rel.RecentContactCount).IsEqualTo(2);
    }

    [Test]
    public async Task RecordContact_DoesNotLowerLastSeenDay()
    {
        var state = new RelationshipState();
        state.RecordContact(NpcId.NurseSalma, 20);
        state.RecordContact(NpcId.NurseSalma, 5);
        await Assert.That(state.GetNpcRelationship(NpcId.NurseSalma).LastSeenDay).IsEqualTo(20);
    }

    [Test]
    public async Task SetDebtState_UpdatesFlag()
    {
        var state = new RelationshipState();
        state.SetDebtState(NpcId.LandlordHajjMahmoud, true);
        await Assert.That(state.GetNpcRelationship(NpcId.LandlordHajjMahmoud).HasUnpaidDebt).IsTrue();
        state.SetDebtState(NpcId.LandlordHajjMahmoud, false);
        await Assert.That(state.GetNpcRelationship(NpcId.LandlordHajjMahmoud).HasUnpaidDebt).IsFalse();
    }

    [Test]
    public async Task SetEmbarrassedState_UpdatesFlag()
    {
        var state = new RelationshipState();
        state.SetEmbarrassedState(NpcId.CafeOwnerNadia, true);
        await Assert.That(state.GetNpcRelationship(NpcId.CafeOwnerNadia).WasEmbarrassed).IsTrue();
    }

    [Test]
    public async Task SetHelpedState_UpdatesFlag()
    {
        var state = new RelationshipState();
        state.SetHelpedState(NpcId.FenceHanan, true);
        await Assert.That(state.GetNpcRelationship(NpcId.FenceHanan).WasHelped).IsTrue();
    }

    [Test]
    public async Task RecordSeenConversation_AddsKnot()
    {
        var state = new RelationshipState();
        state.RecordSeenConversation(NpcId.NeighborMona, "mona_default_1");
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "mona_default_1")).IsTrue();
    }

    [Test]
    public async Task RecordSeenConversation_IgnoresEmptyKnot()
    {
        var state = new RelationshipState();
        state.RecordSeenConversation(NpcId.NeighborMona, "");
        state.RecordSeenConversation(NpcId.NeighborMona, "   ");
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "")).IsFalse();
    }

    [Test]
    public async Task HasSeenConversation_ReturnsFalseForUnseen()
    {
        var state = new RelationshipState();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "mona_default_1")).IsFalse();
    }

    [Test]
    public async Task HasSeenConversation_IgnoresEmptyKnot()
    {
        var state = new RelationshipState();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "")).IsFalse();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, null!)).IsFalse();
    }

    [Test]
    public async Task RestoreConversationHistory_ReplacesAll()
    {
        var state = new RelationshipState();
        state.RecordSeenConversation(NpcId.NeighborMona, "old_knot");
        state.RestoreConversationHistory(NpcId.NeighborMona, ["new_knot_1", "new_knot_2"]);
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "old_knot")).IsFalse();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "new_knot_1")).IsTrue();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "new_knot_2")).IsTrue();
    }

    [Test]
    public async Task RestoreConversationHistory_FiltersEmptyStrings()
    {
        var state = new RelationshipState();
        state.RestoreConversationHistory(NpcId.NeighborMona, ["valid", "", "   ", "also_valid"]);
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "valid")).IsTrue();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "also_valid")).IsTrue();
        await Assert.That(state.HasSeenConversation(NpcId.NeighborMona, "")).IsFalse();
    }

    [Test]
    public async Task SetNpcRelationshipMemory_SetsAllFields()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NurseSalma, 10, 5, true, false, true, 3);
        var rel = state.GetNpcRelationship(NpcId.NurseSalma);
        await Assert.That(rel.LastFavorDay).IsEqualTo(10);
        await Assert.That(rel.LastRefusalDay).IsEqualTo(5);
        await Assert.That(rel.HasUnpaidDebt).IsTrue();
        await Assert.That(rel.WasEmbarrassed).IsFalse();
        await Assert.That(rel.WasHelped).IsTrue();
        await Assert.That(rel.RecentContactCount).IsEqualTo(3);
    }

    [Test]
    public async Task SetNpcRelationshipMemory_ClampsNegativeDaysToZero()
    {
        var state = new RelationshipState();
        state.SetNpcRelationshipMemory(NpcId.NurseSalma, -5, -3, false, false, false, -1);
        var rel = state.GetNpcRelationship(NpcId.NurseSalma);
        await Assert.That(rel.LastFavorDay).IsEqualTo(0);
        await Assert.That(rel.LastRefusalDay).IsEqualTo(0);
        await Assert.That(rel.RecentContactCount).IsEqualTo(0);
    }

    [Test]
    public async Task NpcRelationships_ContainsAllNpcIds()
    {
        var state = new RelationshipState();
        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var rel = state.GetNpcRelationship(npcId);
            await Assert.That(rel).IsNotNull();
        }
    }
}
