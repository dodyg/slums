using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Narrative;
using Slums.Core.State;
using Slums.Core.State.DailyResolution;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Community;

internal sealed class CommunityEventServiceTests
{
    [Test]
    public void GetAvailable_ShouldFilterInvitationOnlyEvents()
    {
        var session = TestSessions.Create();

        CommunityEventService.GetAvailable(session)
            .Should().NotContain(static definition => definition.Id == CommunityEventId.RooftopTeaCircle);

        session.EventAttendance.HasTeaCircleInvitation = true;

        CommunityEventService.GetAvailable(session)
            .Should().Contain(static definition => definition.Id == CommunityEventId.RooftopTeaCircle);
    }

    [Test]
    public void RequestEmergencySupport_ShouldClaimOnlyOnce()
    {
        var session = TestSessions.Create();
        session.Player.ApplyBackground(TestContent.Catalog.GetBackground(BackgroundType.MedicalSchoolDropout));

        CommunityEventService.RequestEmergencySupport(session).Should().BeTrue();
        CommunityEventService.RequestEmergencySupport(session).Should().BeFalse();
        session.HasClaimedEmergencySupport.Should().BeTrue();
    }

    [Test]
    public void Mulid_ShouldOnlyBeAvailableOnItsAnchorDays()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(22, 8, 0);

        CommunityEventService.GetAvailable(session).Should().NotContain(static item => item.Id == CommunityEventId.MulidFestival);

        session.Clock.SetTime(23, 8, 0);
        CommunityEventService.GetAvailable(session).Should().Contain(static item => item.Id == CommunityEventId.MulidFestival);

        session.Clock.SetTime(25, 8, 0);
        CommunityEventService.GetAvailable(session).Should().NotContain(static item => item.Id == CommunityEventId.MulidFestival);
    }

    [Test]
    public void MulidAttendance_ShouldRecordHolidayWitnessFlag()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(23, 8, 0);
        session.Player.Stats.SetMoney(100);

        CommunityEventService.Attend(session, CommunityEventId.MulidFestival, new Random(7)).Should().BeTrue();

        session.HasStoryFlag(StoryFlags.HolidayMulidWitnessed).Should().BeTrue();
    }

    [Test]
    public void HolidayEffect_ShouldRecordWitnessFlag()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(99, 8, 0);

        DailyStatResolution.ApplyDecayAndRecovery(session, 15);

        session.HasStoryFlag(StoryFlags.HolidayCopticChristmasWitnessed).Should().BeTrue();
    }
}
