using FluentAssertions;
using Slums.Core.Diagnostics;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Diagnostics;

internal sealed class GameSessionMutationTests
{
    [Test]
    public void Mutations_ShouldBeEmptyOnNewSession()
    {
        using var session = new GameSession();

        session.Mutations.Should().BeEmpty();
    }

    [Test]
    public void RestAtHome_ShouldRecordMutation_WhenSuccessful()
    {
        using var session = new GameSession();
        session.World.TravelTo(LocationId.Home);

        var result = session.RestAtHome();

        result.Should().BeTrue();
        session.Mutations.Should().ContainSingle(m =>
            m.Category == MutationCategories.Rest &&
            m.Action == "RestAtHome");
    }

    [Test]
    public void RestAtHome_ShouldRecordGuardRejection_WhenNotAtHome()
    {
        using var session = new GameSession();
        session.World.TravelTo(LocationId.Market);

        var result = session.RestAtHome();

        result.Should().BeFalse();
        session.Mutations.Should().ContainSingle(m =>
            m.Category == MutationCategories.GuardRejected &&
            m.Action == "RestAtHome" &&
            m.Reason.Contains("Not at home"));
    }

    [Test]
    public void BuyFood_ShouldRecordMutation_WhenSuccessful()
    {
        using var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        session.Player.Stats.ModifyMoney(1000);

        var result = session.BuyFood();

        result.Should().BeTrue();
        session.Mutations.Should().ContainSingle(m =>
            m.Category == MutationCategories.Food &&
            m.Action == "BuyFood");
    }

    [Test]
    public void BuyFood_ShouldRecordGuardRejection_WhenNotEnoughMoney()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(0);

        var result = session.BuyFood();

        result.Should().BeFalse();
        session.Mutations.Should().ContainSingle(m =>
            m.Category == MutationCategories.GuardRejected &&
            m.Action == "BuyFood" &&
            m.Reason.Contains("Not enough money"));
    }

    [Test]
    public void EatAtHome_ShouldRecordGuardRejection_WhenNoFood()
    {
        using var session = new GameSession();
        session.Player.Household.SetStaplesUnits(0);

        var result = session.EatAtHome();

        result.Should().BeFalse();
        session.Mutations.Should().ContainSingle(m =>
            m.Category == MutationCategories.GuardRejected &&
            m.Action == "EatAtHome" &&
            m.Reason.Contains("Not enough food"));
    }

    [Test]
    public void EndDay_ShouldRecordDayTransitionMutation()
    {
        using var session = new GameSession();

        session.EndDay();

        session.Mutations.Should().Contain(m =>
            m.Category == MutationCategories.DayTransition &&
            m.Action == "EndDay");
    }

    [Test]
    public void MutationRecords_ShouldCarryRunId()
    {
        using var session = new GameSession();

        session.EndDay();

        session.Mutations.All(m => m.RunId == session.RunId).Should().BeTrue();
    }

    [Test]
    public void MutationRecords_ShouldCaptureBeforeAndAfterStats()
    {
        using var session = new GameSession();

        session.EndDay();

        session.Mutations.Should().Contain(m =>
            m.Before.ContainsKey("Money") &&
            m.Before.ContainsKey("Energy") &&
            m.Before.ContainsKey("Day") &&
            m.After.ContainsKey("Money") &&
            m.After.ContainsKey("Day") &&
            m.After["Day"]!.ToString() == "2");
    }

    [Test]
    public void MutationRecorded_ShouldFire_WhenMutationIsRecorded()
    {
        using var session = new GameSession();
        var eventCount = 0;
        GameMutationEventArgs? capturedArgs = null;
        session.MutationRecorded += (_, e) =>
        {
            eventCount++;
            capturedArgs = e;
        };

        session.Player.Stats.SetMoney(0);
        session.BuyFood();

        eventCount.Should().Be(1, "GuardRejected BuyFood should still fire the event");
        capturedArgs!.Record.Category.Should().Be(MutationCategories.GuardRejected);

        session.Player.Stats.SetMoney(1000);
        session.BuyFood();

        eventCount.Should().Be(2, "Successful BuyFood should fire the event again");
        capturedArgs.Record.Category.Should().Be(MutationCategories.Food);
    }
}
