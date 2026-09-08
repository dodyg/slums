using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Community;
using Slums.Core.State;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Application.Tests.Activities;

internal sealed class CommunityEventMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldIncludeCurrentMinuteWhenCheckingRemainingTime()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(day: 7, hour: 20, minute: 30);
        var query = new CommunityEventMenuQuery();

        var statuses = query.GetStatuses(CommunityEventMenuContext.Create(session));

        var fridayGathering = statuses.Single(status => status.Event.Id == CommunityEventId.FridayRooftopGathering);
        fridayGathering.HasTime.Should().BeFalse();
        fridayGathering.CanAttend.Should().BeFalse();
        fridayGathering.UnavailabilityReason.Should().Contain("Not enough time");
    }

    [Test]
    public void GetStatuses_ShouldExplainSeasonalAnchorWhenMulidIsUnavailable()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(25, 8, 0);

        var status = new CommunityEventMenuQuery()
            .GetStatuses(CommunityEventMenuContext.Create(session))
            .Single(item => item.Event.Id == CommunityEventId.MulidFestival);

        status.CanAttend.Should().BeFalse();
        status.UnavailabilityReason.Should().Contain("days 23, 24, 110, 111");
    }
}
