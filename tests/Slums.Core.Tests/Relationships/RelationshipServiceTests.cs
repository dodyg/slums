using FluentAssertions;
using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Relationships;

internal sealed class RelationshipServiceTests
{
    [Test]
    public void ModifyTrust_ShouldClampAtBounds()
    {
        var state = new RelationshipState();

        RelationshipService.ModifyTrust(state, NpcId.LandlordHajjMahmoud, 500, 3);

        state.GetNpcRelationship(NpcId.LandlordHajjMahmoud).Trust.Should().Be(100);
    }

    [Test]
    public void ModifyTrust_ShouldReturnEvent_WhenThresholdIsCrossed()
    {
        var state = new RelationshipState();

        var message = RelationshipService.ModifyTrust(state, NpcId.LandlordHajjMahmoud, -60, 3);

        message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ModifyReputation_ShouldNotChangeNpcTrust()
    {
        var state = new RelationshipState();

        RelationshipService.ModifyReputation(state, FactionId.ImbabaCrew, 25);

        state.GetFactionStanding(FactionId.ImbabaCrew).Reputation.Should().Be(25);
        state.GetNpcRelationship(NpcId.FixerUmmKarim).Trust.Should().Be(0);
    }

    [Test]
    public void RelationshipState_ShouldRecordFavorRefusalAndFlags()
    {
        var state = new RelationshipState();

        state.RecordFavor(NpcId.NurseSalma, currentDay: 6, hasUnpaidDebt: true);
        state.RecordRefusal(NpcId.FixerUmmKarim, currentDay: 7);
        state.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
        state.SetHelpedState(NpcId.NeighborMona, true);

        state.GetNpcRelationship(NpcId.NurseSalma).LastFavorDay.Should().Be(6);
        state.GetNpcRelationship(NpcId.NurseSalma).HasUnpaidDebt.Should().BeTrue();
        state.GetNpcRelationship(NpcId.FixerUmmKarim).LastRefusalDay.Should().Be(7);
        state.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).WasEmbarrassed.Should().BeTrue();
        state.GetNpcRelationship(NpcId.NeighborMona).WasHelped.Should().BeTrue();
    }
}