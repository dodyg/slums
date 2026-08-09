using Slums.Core.Clock;
using TUnit.Core;

namespace Slums.Core.Tests.Clock;

internal sealed class DailyActivityWindowTests
{
    [Test]
    public async Task RemainingMinutes_ShouldIncludeCurrentMinute()
    {
        var clock = new GameClock();
        clock.SetTime(day: 1, hour: 21, minute: 30);

        var remaining = DailyActivityWindow.GetRemainingMinutes(clock, endOfDayHour: 22);

        await Assert.That(remaining).IsEqualTo(30);
        await Assert.That(DailyActivityWindow.CanComplete(clock, 30, endOfDayHour: 22)).IsTrue();
        await Assert.That(DailyActivityWindow.CanComplete(clock, 31, endOfDayHour: 22)).IsFalse();
    }
}
