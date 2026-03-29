using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Tests.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Diagnostics;

internal sealed class AutoTransactionEventLogTests
{
    [Test]
    public void EndDay_ShouldRaiseAutoTransactionEvent_WhenRentIsPaid()
    {
        using var session = new GameSession();
        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.EndDay();

        events.Should().Contain(e =>
            e.StartsWith("[Day 1]", StringComparison.Ordinal) &&
            e.Contains("Paid rent") &&
            e.Contains("20 LE"));
    }

    [Test]
    public void EndDay_ShouldRaiseAutoTransactionEvent_WhenRentCannotBePaid()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(0);
        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.EndDay();

        events.Should().Contain(e =>
            e.StartsWith("[Day 1]", StringComparison.Ordinal) &&
            e.Contains("Could not pay rent"));
    }

    [Test]
    public void EndDay_ShouldRaiseAutoTransactionEvent_WhenHerbsAreSold()
    {
        using var session = new GameSession();
        session.World.TravelTo(LocationId.PlantShop);
        session.Player.Stats.SetMoney(1000);
        session.BuyPlant(PlantType.Chamomile);
        session.World.TravelTo(LocationId.Home);

        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.EndDay();
        session.EndDay();
        session.EndDay();
        session.EndDay();
        session.EndDay();
        session.EndDay();

        events.Should().Contain(e =>
            e.StartsWith("[Day 6]", StringComparison.Ordinal) &&
            e.Contains("street vendor") &&
            e.Contains("LE"));
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldRaiseAutoTransactionEvent_PerInvestmentIncome()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(300);
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);
        session.MakeInvestment(InvestmentType.FoulCart);

        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [10]));

        events.Should().Contain(e =>
            e.StartsWith("[Day 1]", StringComparison.Ordinal) &&
            e.Contains("weekly income") &&
            e.Contains("LE"));
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldRaiseAutoTransactionEvent_ForWeeklySummary()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(300);
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);
        session.MakeInvestment(InvestmentType.FoulCart);

        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [10]));

        events.Should().Contain(e =>
            e.StartsWith("[Day 1]", StringComparison.Ordinal) &&
            e.Contains("Weekly investments"));
    }

    [Test]
    public void EndDay_ShouldRaiseAutoTransactionEvent_ForInvestmentResolutionOnMonday()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(500);
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);
        session.MakeInvestment(InvestmentType.FoulCart);

        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.EndDay();
        session.EndDay();

        events.Should().Contain(e =>
            e.StartsWith("[Day 3]", StringComparison.Ordinal) &&
            e.Contains("investment", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void AutoTransactionEvents_ShouldIncludeCorrectDayNumber()
    {
        using var session = new GameSession();
        var events = new List<string>();
        session.GameEvent += (_, e) => events.Add(e.Message);

        session.EndDay();

        var autoTransactions = events.Where(e => e.StartsWith("[Day ", StringComparison.Ordinal)).ToList();
        autoTransactions.Should().NotBeEmpty();
        foreach (var evt in autoTransactions)
        {
            var dayMatch = System.Text.RegularExpressions.Regex.Match(evt, @"\[Day (\d+)\]");
            dayMatch.Success.Should().BeTrue();
            int.Parse(dayMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture).Should().Be(1);
        }
    }
}
