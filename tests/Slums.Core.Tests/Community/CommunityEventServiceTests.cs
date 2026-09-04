using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Community;

internal sealed class CommunityEventServiceTests
{
    [Test]
    public void GetAvailable_ShouldFilterInvitationOnlyEvents()
    {
        var session = new GameSession();

        CommunityEventService.GetAvailable(session)
            .Should().NotContain(static definition => definition.Id == CommunityEventId.RooftopTeaCircle);

        session.EventAttendance.HasTeaCircleInvitation = true;

        CommunityEventService.GetAvailable(session)
            .Should().Contain(static definition => definition.Id == CommunityEventId.RooftopTeaCircle);
    }

    [Test]
    public void RequestEmergencySupport_ShouldClaimOnlyOnce()
    {
        var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);

        CommunityEventService.RequestEmergencySupport(session).Should().BeTrue();
        CommunityEventService.RequestEmergencySupport(session).Should().BeFalse();
        session.HasClaimedEmergencySupport.Should().BeTrue();
    }
}
