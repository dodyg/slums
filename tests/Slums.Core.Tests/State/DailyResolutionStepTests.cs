using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.State.DailyResolution;
using Slums.Core.Tests.Investments;
using TUnit;
using Slums.TestSupport;

namespace Slums.Core.Tests.State;

internal sealed class DailyResolutionStepTests
{
    [Test]
    public async Task ApplyBackgroundAndGenderStress_MedicalDropoutWithFragileMother_AddsThreeStress()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        session.Player.ApplyBackground(TestContent.Catalog.GetBackground(BackgroundType.MedicalSchoolDropout));
        session.Player.ApplyGender(Gender.Male);
        session.Player.Household.SetMotherHealth(50);
        var stressBefore = session.Player.Stats.Stress;

        DailyStatResolution.ApplyBackgroundAndGenderStress(session);

        await Assert.That(session.Player.Stats.Stress).IsEqualTo(stressBefore + 3);
    }

    [Test]
    public async Task ApplyBackgroundAndGenderStress_FemaleProtagonist_AddsDailyStress()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        session.Player.ApplyGender(Gender.Female);
        var stressBefore = session.Player.Stats.Stress;

        DailyStatResolution.ApplyBackgroundAndGenderStress(session);

        await Assert.That(session.Player.Stats.Stress).IsEqualTo(stressBefore + 1);
    }

    [Test]
    public async Task ProcessRent_WithEnoughMoney_PaysRentAndJournalsTheTransaction()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        var moneyBefore = session.Player.Stats.Money;

        DailyEconomyResolution.ProcessRent(session);

        await Assert.That(session.Player.Stats.Money).IsEqualTo(moneyBefore - 20);
        await Assert.That(session.EventJournal.Entries.Any(entry => entry.Message.EndsWith("Paid rent: 20 LE", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task ProcessRent_WithoutMoney_AccumulatesRentDebt()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        session.Player.Stats.SetMoney(0);

        DailyEconomyResolution.ProcessRent(session);

        await Assert.That(session.UnpaidRentDays).IsEqualTo(1);
        await Assert.That(session.AccumulatedRentDebt).IsEqualTo(20);
    }

    [Test]
    public async Task ResolveAttendance_OnTheFirstDay_RecordsTheInitialSkip()
    {
        var session = TestSessions.Create(new GameRandom(20260904));

        DailyInformationResolution.ResolveAttendance(session);

        await Assert.That(session.EventAttendance.ConsecutiveSkips).IsEqualTo(1);
    }

    [Test]
    public void ResolveWeeklyCycle_ShouldSplitMondayAndWednesdayBlocks()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        session.Player.Stats.SetMoney(500);
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);
        session.MakeInvestment(InvestmentType.FoulCart);

        session.Clock.SetTime(3, 6, 0);
        DailyEconomyResolution.ResolveWeeklyCycle(session, session.SharedRandom);
        session.TotalInvestmentEarnings.Should().Be(0);

        session.Clock.SetTime(5, 6, 0);
        DailyEconomyResolution.ResolveWeeklyCycle(session, new SequenceRandom(intValues: [10]));

        session.TotalInvestmentEarnings.Should().Be(17);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message.Contains("+17 LE weekly income", StringComparison.Ordinal));
    }
}
