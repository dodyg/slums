using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class CrimeTimeTests
{
    [Test]
    public void CommitCrime_ShouldAdvanceTheClockByTheRouteDuration()
    {
        var session = new GameSession();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 0, 10, 0, 10);
        var before = (session.Clock.Hour * 60) + session.Clock.Minute;

        session.CommitCrime(attempt, new Random(1));

        var after = (session.Clock.Hour * 60) + session.Clock.Minute;
        after.Should().Be(before + attempt.DurationMinutes);
    }

    [Test]
    public void CommitCrime_ShouldApplyCrimeEffectsBeforeCrossingIntoTheNextDay()
    {
        var session = new GameSession();
        session.Clock.SetTime(1, 21, 30);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 0, 10, 0, 10);

        session.CommitCrime(attempt, new Random(1));

        session.Clock.Day.Should().Be(2);
        session.Clock.Hour.Should().Be(6);
        session.Clock.Minute.Should().Be(30);
        session.CrimesCommitted.Should().Be(1);
        session.LastCrimeDay.Should().Be(1);
    }

}
