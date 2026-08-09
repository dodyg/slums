using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Community;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class CommunityEventMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldIncludeCurrentMinuteWhenCheckingRemainingTime()
    {
        var session = new GameSession();
        session.Clock.SetTime(day: 7, hour: 20, minute: 30);
        var query = new CommunityEventMenuQuery();

        var statuses = query.GetStatuses(CommunityEventMenuContext.Create(session));

        var fridayGathering = statuses.Single(status => status.Event.Id == CommunityEventId.FridayRooftopGathering);
        fridayGathering.HasTime.Should().BeFalse();
        fridayGathering.CanAttend.Should().BeFalse();
        fridayGathering.UnavailabilityReason.Should().Contain("Not enough time");
    }
}
