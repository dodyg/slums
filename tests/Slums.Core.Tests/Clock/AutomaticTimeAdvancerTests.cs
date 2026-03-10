using FluentAssertions;
using Slums.Core.Clock;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.Clock;

public sealed class AutomaticTimeAdvancerTests
{
    [Test]
    public async Task CollectElapsedMinutes_ShouldReturnZeroUntilThresholdReached()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var elapsedMinutes = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(999));

        await Assert.That(elapsedMinutes).IsEqualTo(0);
    }

    [Test]
    public async Task CollectElapsedMinutes_ShouldCarryBufferedTimeAcrossUpdates()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var firstUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(600));
        var secondUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(600));
        var thirdUpdate = advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(800));

        await Assert.That(firstUpdate).IsEqualTo(0);
        await Assert.That(secondUpdate).IsEqualTo(1);
        await Assert.That(thirdUpdate).IsEqualTo(1);
    }

    [Test]
    public async Task CollectElapsedMinutes_ShouldReturnMultipleMinutesForLargeDelta()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var elapsedMinutes = advancer.CollectElapsedMinutes(TimeSpan.FromSeconds(3.5));

        await Assert.That(elapsedMinutes).IsEqualTo(3);
    }

    [Test]
    public async Task CollectElapsedMinutes_ShouldRejectNegativeDelta()
    {
        var advancer = new AutomaticTimeAdvancer(TimeSpan.FromSeconds(1));

        var act = () => advancer.CollectElapsedMinutes(TimeSpan.FromMilliseconds(-1));

        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }
}
