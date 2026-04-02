using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingServiceCoverageTests
{
    [Test]
    public async Task CheckEndings_ReturnsDestitution_WhenStarvingExhaustedAndBroke()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(0);
        state.Player.Stats.SetHunger(5);
        state.Player.Stats.SetEnergy(5);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Destitution);
    }

    [Test]
    public async Task CheckEndings_DoesNotReturnDestitution_WhenMoneyAboveZero()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(1);
        state.Player.Stats.SetHunger(95);
        state.Player.Stats.SetEnergy(5);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ReturnsCrimeKingpin_WhenCrimeEarningsAndRepHigh()
    {
        using var state = new GameSession();
        state.SetCrimeCounters(1000, 10);
        state.Relationships.SetFactionStanding(FactionId.ImbabaCrew, 55);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.CrimeKingpin);
    }

    [Test]
    public async Task CheckEndings_DoesNotReturnCrimeKingpin_WhenRepTooLow()
    {
        using var state = new GameSession();
        state.SetCrimeCounters(1000, 10);
        state.Relationships.SetFactionStanding(FactionId.ImbabaCrew, 40);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_DoesNotReturnCrimeKingpin_WhenEarningsTooLow()
    {
        using var state = new GameSession();
        state.SetCrimeCounters(500, 10);
        state.Relationships.SetFactionStanding(FactionId.ImbabaCrew, 60);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ReturnsQuitTheLuxorDream_WhenCriteriaMet()
    {
        using var state = new GameSession();
        state.SetDaysSurvived(30);
        state.Player.Stats.SetMoney(550);
        state.SetCrimeCounters(100, 2, lastCrimeDay: 20);
        state.Player.Household.SetMotherHealth(70);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.QuitTheLuxorDream);
    }

    [Test]
    public async Task CheckEndings_DoesNotReturnLuxor_WhenTooManyCrimes()
    {
        using var state = new GameSession();
        state.SetDaysSurvived(30);
        state.Player.Stats.SetMoney(550);
        state.SetCrimeCounters(100, 5, lastCrimeDay: 20);
        state.Player.Household.SetMotherHealth(70);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_DoesNotReturnLuxor_WhenMotherHealthLow()
    {
        using var state = new GameSession();
        state.SetDaysSurvived(30);
        state.Player.Stats.SetMoney(550);
        state.SetCrimeCounters(100, 2, lastCrimeDay: 20);
        state.Player.Household.SetMotherHealth(50);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_MotherDiedTakesPriorityOverArrested()
    {
        using var state = new GameSession();
        state.SetPolicePressure(100);
        state.Player.Household.SetMotherHealth(0);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.MotherDied);
    }

    [Test]
    public async Task CheckEndings_CollapseTakesPriorityOverDestitution()
    {
        using var state = new GameSession();
        state.Player.Stats.SetHealth(0);
        state.Player.Stats.SetMoney(0);
        state.Player.Stats.SetHunger(5);
        state.Player.Stats.SetEnergy(5);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.CollapseFromExhaustion);
    }

    [Test]
    public async Task CheckEndings_DestitutionTakesPriorityOverArrested()
    {
        using var state = new GameSession();
        state.SetPolicePressure(100);
        state.Player.Stats.SetMoney(0);
        state.Player.Stats.SetHunger(5);
        state.Player.Stats.SetEnergy(5);
        state.Player.Stats.SetHealth(30);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Destitution);
    }

    [Test]
    public async Task CheckEndings_ArrestedTakesPriorityOverEviction()
    {
        using var state = new GameSession();
        state.SetPolicePressure(100);
        state.RestoreRentState(unpaidRentDays: 7, accumulatedRentDebt: 140, firstWarningGiven: true, finalWarningGiven: true);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }

    [Test]
    public async Task GetMessage_ReturnsNonNullForAllEndings()
    {
        foreach (var endingId in Enum.GetValues<EndingId>())
        {
            var message = EndingService.GetMessage(endingId);
            await Assert.That(message).IsNotNull();
            await Assert.That(message.Length).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task GetInkKnot_ReturnsNonNullForAllEndings()
    {
        using var state = new GameSession();
        foreach (var endingId in Enum.GetValues<EndingId>())
        {
            var knot = EndingService.GetInkKnot(state, endingId);
            await Assert.That(knot).IsNotNull();
            await Assert.That(knot.Length).IsGreaterThan(0);
        }
    }
}
