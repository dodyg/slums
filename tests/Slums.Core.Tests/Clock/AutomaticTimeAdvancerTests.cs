using FluentAssertions;
using Slums.Core.Clock;
using Xunit;

namespace Slums.Core.Tests.Clock;

public sealed class AutomaticTimeAdvancerTests
{
    [Fact]
    public void CollectElapsedMinutes_ShouldReturnZeroUntilThresholdReached()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var elapsedMinutes = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(999));

        elapsedMinutes.Should().Be(0);
    }

    [Fact]
    public void CollectElapsedMinutes_ShouldCarryBufferedTimeAcrossUpdates()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var firstUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(600));
        var secondUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(600));
        var thirdUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(800));

        firstUpdate.Should().Be(0);
        secondUpdate.Should().Be(1);
        thirdUpdate.Should().Be(1);
    }

    [Fact]
    public void CollectElapsedMinutes_ShouldReturnMultipleMinutesForLargeDelta()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var elapsedMinutes = advancer.CollectElapsedMinutes(TimeSpan.FromSeconds(3.5));

        elapsedMinutes.Should().Be(3);
    }

    [Fact]
    public void CollectElapsedMinutes_ShouldRejectNegativeDelta()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var act = () => advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
