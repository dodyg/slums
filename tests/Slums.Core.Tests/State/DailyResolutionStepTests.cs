using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.State;
using Slums.Core.State.DailyResolution;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class DailyResolutionStepTests
{
    [Test]
    public async Task ApplyBackgroundAndGenderStress_MedicalDropoutWithFragileMother_AddsThreeStress()
    {
        var session = new GameSession(new GameRandom(20260904));
        session.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        session.Player.Gender = Gender.Male;
        session.Player.Household.SetMotherHealth(50);
        var stressBefore = session.Player.Stats.Stress;

        DailyStatResolution.ApplyBackgroundAndGenderStress(session);

        await Assert.That(session.Player.Stats.Stress).IsEqualTo(stressBefore + 3);
    }

    [Test]
    public async Task ApplyBackgroundAndGenderStress_FemaleProtagonist_AddsDailyStress()
    {
        var session = new GameSession(new GameRandom(20260904));
        session.Player.Gender = Gender.Female;
        var stressBefore = session.Player.Stats.Stress;

        DailyStatResolution.ApplyBackgroundAndGenderStress(session);

        await Assert.That(session.Player.Stats.Stress).IsEqualTo(stressBefore + 1);
    }

    [Test]
    public async Task ProcessRent_WithEnoughMoney_PaysRentAndJournalsTheTransaction()
    {
        var session = new GameSession(new GameRandom(20260904));
        var moneyBefore = session.Player.Stats.Money;

        DailyEconomyResolution.ProcessRent(session);

        await Assert.That(session.Player.Stats.Money).IsEqualTo(moneyBefore - 20);
        await Assert.That(session.EventJournal.Entries.Any(entry => entry.Message.EndsWith("Paid rent: 20 LE", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task ProcessRent_WithoutMoney_AccumulatesRentDebt()
    {
        var session = new GameSession(new GameRandom(20260904));
        session.Player.Stats.SetMoney(0);

        DailyEconomyResolution.ProcessRent(session);

        await Assert.That(session.UnpaidRentDays).IsEqualTo(1);
        await Assert.That(session.AccumulatedRentDebt).IsEqualTo(20);
    }

    [Test]
    public async Task ResolveAttendance_OnTheFirstDay_RecordsTheInitialSkip()
    {
        var session = new GameSession(new GameRandom(20260904));

        DailyInformationResolution.ResolveAttendance(session);

        await Assert.That(session.EventAttendance.ConsecutiveSkips).IsEqualTo(1);
    }
}
