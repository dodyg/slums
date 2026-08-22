using FluentAssertions;
using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingChoiceTests
{
    [Test]
    public void AvailableEnding_ShouldBeAChoiceAndNotAnAutomaticGameOver()
    {
        var session = CreateStableSession();

        session.GetAvailableEndingChoices().Should().Contain(EndingId.StabilityHonestWork);
        session.IsGameOver.Should().BeFalse();
        session.EndingId.Should().BeNull();
    }

    [Test]
    public void TryChooseEnding_ShouldOpenPendingCommitment()
    {
        var session = CreateStableSession();

        var chosen = session.TryChooseEnding(EndingId.StabilityHonestWork);

        chosen.Should().BeTrue();
        session.IsGameOver.Should().BeFalse();
        session.EndingId.Should().BeNull();
        session.PendingEndingId.Should().Be(EndingId.StabilityHonestWork);
        session.PendingEndingKnot.Should().Be(EndingKnotCatalog.Commitment);

        session.CommitEnding(EndingId.StabilityHonestWork, "care_shift");

        session.IsGameOver.Should().BeTrue();
        session.EndingId.Should().Be(EndingId.StabilityHonestWork);
        session.FinalSacrifice.Should().Be("care_shift");
        session.PendingEndingKnot.Should().Be("ending_stability_medical");
    }

    [Test]
    public void LuxorChoice_ShouldRequireTheConcreteTrainFare()
    {
        var session = new GameSession();
        session.SetDaysSurvived(30);
        session.Clock.SetTime(30, 8, 0);
        session.Player.Stats.SetMoney(549);
        session.Player.Household.SetMotherHealth(70);

        session.GetAvailableEndingChoices().Should().NotContain(EndingId.QuitTheLuxorDream);

        session.Player.Stats.SetMoney(550);

        session.GetAvailableEndingChoices().Should().Contain(EndingId.QuitTheLuxorDream);
    }

    private static GameSession CreateStableSession()
    {
        var session = new GameSession();
        session.SetDaysSurvived(30);
        session.Clock.SetTime(30, 8, 0);
        session.SetWorkCounters(180, 6, 30, 30);
        session.SetCrimeCounters(0, 0, 0);
        session.SetPolicePressure(10);
        return session;
    }
}
