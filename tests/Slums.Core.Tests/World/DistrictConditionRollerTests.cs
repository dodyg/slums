using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.World;

internal sealed class DistrictConditionRollerTests
{
    [Test]
    public void SetBaseline_ShouldProvideOneConditionPerDistrict()
    {
        var session = TestSessions.Create();

        DistrictConditionRoller.SetBaseline(session);

        DistrictConditionRoller.GetDailyConditions(session).Should().HaveCount(Enum.GetValues<DistrictId>().Length);
    }

    [Test]
    public void RollForCurrentDay_ShouldBeDeterministicWithTheSameSeed()
    {
        var first = TestSessions.Create();
        var second = TestSessions.Create();

        DistrictConditionRoller.RollForCurrentDay(first, new Random(77));
        DistrictConditionRoller.RollForCurrentDay(second, new Random(77));

        DistrictConditionRoller.GetDailyConditions(second).Select(static definition => definition.Id)
            .Should().Equal(DistrictConditionRoller.GetDailyConditions(first).Select(static definition => definition.Id));
    }
}
