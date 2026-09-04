using FluentAssertions;
using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingCommitmentServiceTests
{
    [Test]
    public void TryChooseEnding_ShouldPersistThePendingCommitment()
    {
        var session = CreateStableSession();

        var chosen = EndingCommitmentService.TryChooseEnding(session, EndingId.StabilityHonestWork);

        chosen.Should().BeTrue();
        session.PendingEndingId.Should().Be(EndingId.StabilityHonestWork);
        session.PendingEndingKnot.Should().Be(EndingKnotCatalog.Commitment);
    }

    [Test]
    public void CheckGameOverConditions_ShouldCommitAutomaticFailureEndings()
    {
        var session = new GameSession();
        session.SetPolicePressure(100);

        EndingCommitmentService.CheckGameOverConditions(session);

        session.IsGameOver.Should().BeTrue();
        session.EndingId.Should().Be(EndingId.Arrested);
        session.PendingEndingKnot.Should().NotBeNullOrWhiteSpace();
    }

    private static GameSession CreateStableSession()
    {
        var session = new GameSession();
        session.SetDaysSurvived(30);
        session.Clock.SetTime(30, 8, 0);
        session.RestoreWorkState(180, 6, 30, 30);
        session.SetCrimeCounters(0, 0, 0);
        session.SetPolicePressure(10);
        return session;
    }
}
