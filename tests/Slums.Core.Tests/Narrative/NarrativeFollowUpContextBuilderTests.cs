using FluentAssertions;
using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Economy;
using Slums.Core.Home;
using Slums.Core.Clock;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.Weather;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeFollowUpContextBuilderTests
{
    [Test]
    public void BuildReachabilityContext_MapsSessionSignals()
    {
        var session = new GameSession();
        session.Clock.SetTime(151, 8, 0);
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        session.RestoreWeather(WeatherType.Heatwave);
        session.HomeUpgrades.Purchase(HomeUpgrade.Curtain);
        session.Player.Household.SetMotherHealth(42);

        var context = NarrativeFollowUpContextBuilder.BuildReachabilityContext(session);

        context.Day.Should().Be(151);
        context.Weather.Should().Be(WeatherType.Heatwave);
        context.Season.Should().Be(Season.Winter);
        context.Holiday.Should().Be(HolidayId.Ramadan);
        context.HolidayDay.Should().Be(1);
        context.Background.Should().Be(BackgroundType.SudaneseRefugee);
        context.DayOfWeek.Should().Be(GameDayOfWeek.Tuesday);
        context.IsAtHome.Should().BeTrue();
        context.HasCurtain.Should().BeTrue();
        context.MotherHealth.Should().Be(42);
    }

    [Test]
    public void BuildCommunityDebtContext_MapsRelationshipsAndDebtSignals()
    {
        var session = new GameSession();
        session.Clock.SetTime(12, 8, 0);
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        session.EventAttendance.TotalAttended = 4;
        session.EventAttendance.ConsecutiveSkips = 2;
        session.EventAttendance.HasTeaCircleInvitation = true;
        session.SetPolicePressure(55);
        session.SetCrimeCounters(700, 3);
        session.RestoreWorkState(240, 8, 11, 11);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 14, 2);
        session.Relationships.SetNpcRelationshipMemory(NpcId.NeighborMona, 3, 0, false, false, true, 2);
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 12, 2);
        session.Relationships.SetNpcRelationshipMemory(NpcId.RunnerYoussef, 3, 0, false, false, true, 2);
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 9, 2);
        session.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 8, 2);
        session.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            DueDay = 9,
            OriginDay = 1,
            CollectionState = DebtCollectionState.Overdue,
            InterestWeeklyBasisPoints = 2500
        });
        session.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            DueDay = 20,
            OriginDay = 5,
            CollectionState = DebtCollectionState.Current
        });
        session.Territory.SetInfluence(DistrictId.Imbaba, FactionId.ImbabaCrew, 20);
        session.Territory.SetInfluence(DistrictId.Imbaba, FactionId.DokkiThugs, 55);
        session.Territory.ModifyTension(DistrictId.Imbaba, 50);

        var context = NarrativeFollowUpContextBuilder.BuildCommunityDebtContext(session);

        context.Day.Should().Be(12);
        context.DayOfWeek.Should().Be(GameDayOfWeek.Wednesday);
        context.Background.Should().Be(BackgroundType.ReleasedPoliticalPrisoner);
        context.CommunityAttendance.Should().Be(4);
        context.ConsecutiveCommunitySkips.Should().Be(2);
        context.HasTeaCircleInvitation.Should().BeTrue();
        context.PolicePressure.Should().Be(55);
        context.CrimesCommitted.Should().Be(3);
        context.HonestShiftsCompleted.Should().Be(8);
        context.MonaTrust.Should().Be(14);
        context.YoussefTrust.Should().Be(12);
        context.NadiaTrust.Should().Be(9);
        context.MariamTrust.Should().Be(8);
        context.MonaWasHelped.Should().BeTrue();
        context.YoussefWasHelped.Should().BeTrue();
        context.HasLoanSharkDebt.Should().BeTrue();
        context.LoanSharkDaysOverdue.Should().Be(3);
        context.LoanSharkDaysUntilDue.Should().Be(0);
        context.HasNeighborDebt.Should().BeTrue();
        context.ImbabaTension.Should().Be(70);
        context.ImbabaTensionLevel.Should().Be(TensionLevel.High);
        context.ImbabaControlledByDokkiThugs.Should().BeTrue();
        context.ImbabaControlledByExPrisonerNetwork.Should().BeFalse();
    }
}
