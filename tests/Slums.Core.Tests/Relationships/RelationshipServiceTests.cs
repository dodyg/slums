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
}