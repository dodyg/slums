using FluentAssertions;
using Slums.Core.Economy;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.State.DailyResolution;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class WeeklyEconomyResolutionTests
{
    [Test]
    public void StrugglingOrPoorLandlord_AddsTenRentPressure()
    {
        var session = CreateSession();
        session.NpcEconomies.SetWealthLevel(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Poor);

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.AccumulatedRentDebt.Should().Be(10);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "Hajj Mahmoud's money troubles make him meaner about rent. Rent pressure increases.");
    }

    [Test]
    public void StrugglingMona_AddsThreeStress()
    {
        var session = CreateSession();
        session.NpcEconomies.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Struggling);
        var stressBefore = session.Player.Stats.Stress;

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.Player.Stats.Stress.Should().Be(stressBefore + 3);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "Mona is struggling. The worry weighs on you.");
    }

    [Test]
    public void ComfortableUmmKarim_RaisesGiftEvent()
    {
        var session = CreateSession();
        session.NpcEconomies.SetWealthLevel(NpcId.FixerUmmKarim, NpcWealthLevel.Comfortable);

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "Umm Karim is doing well. She slips you an extra portion.");
    }

    [Test]
    public void LoanAlert_IsSuppressedWhenNeedyNpcTrustIsBelowTen()
    {
        var session = CreateSession();
        session.NpcEconomies.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Struggling);

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.EventJournal.Entries.Should().NotContain(entry => entry.Message == "NeighborMona is in rough shape. They could use help.");
    }

    [Test]
    public void LoanAlert_IsRaisedWhenNeedyNpcTrustIsAtLeastTen()
    {
        var session = CreateSession();
        session.NpcEconomies.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Struggling);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 10, 0);

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "NeighborMona is in rough shape. They could use help.");
    }

    [Test]
    public void LoanSharkInterest_IsAppliedAfterWeeklyResolution()
    {
        var session = CreateSession();
        session.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });

        WeeklyEconomyResolution.Resolve(session, HighRollRandom());

        session.PlayerDebts.Debts.Should().ContainSingle().Which.AmountOwed.Should().Be(250);
    }

    private static GameSession CreateSession()
    {
        return new GameSession(new GameRandom(42));
    }

    private static AlwaysHighRandom HighRollRandom()
    {
        return new AlwaysHighRandom();
    }
}
