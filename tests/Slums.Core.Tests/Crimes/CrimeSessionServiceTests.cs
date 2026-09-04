using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeSessionServiceTests
{
    [Test]
    public void GetAvailableCrimes_ShouldUseTheSessionLocation()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Market);

        var crimes = CrimeSessionService.GetAvailableCrimes(session);

        crimes.Should().NotBeEmpty();
    }

    [Test]
    public void CommitCrime_ShouldRecordTheCrimeMutationThroughTheSession()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Market);
        var attempt = CrimeSessionService.GetAvailableCrimes(session)[0];

        var result = CrimeSessionService.CommitCrime(session, attempt, new Random(42));

        result.Should().NotBeNull();
        session.Mutations[^1].Action.Should().Be("CommitCrime");
    }

    [Test]
    public void RestoreCrimeState_ShouldHydrateTheSessionCrimeState()
    {
        var session = new GameSession();

        CrimeSessionService.RestoreCrimeState(session, 30, 120, 2, 5, true);

        session.PolicePressure.Should().Be(30);
        session.TotalCrimeEarnings.Should().Be(120);
        session.CrimesCommitted.Should().Be(2);
        session.LastCrimeDay.Should().Be(5);
        session.HasCrimeCommittedToday.Should().BeTrue();
    }
}
