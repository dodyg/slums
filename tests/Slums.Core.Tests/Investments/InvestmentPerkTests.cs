using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Entertainment;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentPerkTests
{
    [Test]
    public void FoulCart_ShouldReduceStaplePrice_AndStopWhileSuspended()
    {
        var session = CreatePurchasedSession(InvestmentType.FoulCart, 300, LocationId.Home, (NpcId.LandlordHajjMahmoud, 30));
        var baseSession = TestSessions.Create();

        session.GetFoodCost().Should().Be(baseSession.GetFoodCost() - 1);
        session.ActiveInvestments[0].Suspend();
        session.GetFoodCost().Should().Be(baseSession.GetFoodCost());
    }

    [Test]
    public void MicroLaundry_ShouldRaiseImanTrustOnWeeklyResolution()
    {
        var session = CreatePurchasedSession(InvestmentType.MicroLaundry, 300, LocationId.Laundry);
        var trustBefore = session.Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust;

        session.ResolveWeeklyInvestments(SuccessRandom());

        session.Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust.Should().Be(trustBefore + 1);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message.Contains("perk", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void ScrapCollection_ShouldHaveAChanceToRecoverASparePart()
    {
        var session = CreatePurchasedSession(InvestmentType.ScrapCollection, 300, LocationId.Market);
        session.Player.Skills.SetLevel(SkillId.StreetSmarts, 2);

        session.ResolveWeeklyInvestments(new SequenceRandom(
            doubleValues: [0.99, 0.99, 0.99, 0.99, 0.0],
            intValues: [26]));

        session.Player.Robotics.Parts.Should().Be(1);
    }

    [Test]
    public void Kiosk_ShouldReducePhoneRefillPrice_AndStopWhileSuspended()
    {
        var session = CreatePurchasedSession(InvestmentType.Kiosk, 300, LocationId.Market, (NpcId.FixerUmmKarim, 40));
        var baseSession = TestSessions.Create();
        var baseRefillCost = baseSession.Phone.CreditWeekCost;

        session.RefillPhoneCredit().Success.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(47);
        session.ActiveInvestments[0].Suspend();
        session.Phone.Restore(true, 0, 7, false, null, false);
        session.Player.Stats.SetMoney(100);
        session.RefillPhoneCredit().Success.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(100 - baseRefillCost);
    }

    [Test]
    public void MarketStall_ShouldReduceFoodPriceOnlyInPurchaseDistrict()
    {
        var session = CreatePurchasedSession(InvestmentType.MarketStall, 300, LocationId.Square, (NpcId.RunnerYoussef, 25));
        var baseSession = TestSessions.Create();
        baseSession.World.TravelTo(LocationId.Square);
        var discountedDistrictCost = session.GetFoodCost();

        session.World.TravelTo(LocationId.Market);

        discountedDistrictCost.Should().Be(baseSession.GetFoodCost() - 1);
        baseSession.World.TravelTo(LocationId.Market);
        session.GetFoodCost().Should().Be(baseSession.GetFoodCost());
    }

    [Test]
    public void TeaCart_ShouldOpenTheTeaInvitationDuringWeeklyResolution()
    {
        var session = CreatePurchasedSession(InvestmentType.TeaCart, 200, LocationId.Home, (NpcId.NeighborMona, 10));

        session.ResolveWeeklyInvestments(SuccessRandom());

        session.EventAttendance.HasTeaCircleInvitation.Should().BeTrue();
    }

    [Test]
    public void HerbalRemedyTrade_ShouldReduceMedicinePrice()
    {
        var session = CreatePurchasedSession(InvestmentType.HerbalRemedyTrade, 500, LocationId.Pharmacy, (NpcId.PharmacistMariam, 15));
        session.Player.Skills.SetLevel(SkillId.Medical, 2);
        var withoutPerk = TestSessions.Create();
        withoutPerk.World.TravelTo(LocationId.Pharmacy);
        withoutPerk.Player.Skills.SetLevel(SkillId.Medical, 2);
        withoutPerk.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);

        session.GetMedicineCost().Should().Be(withoutPerk.GetMedicineCost() - 5);
    }

    [Test]
    public void SewingSideBusiness_ShouldShowThePayBonusInWorkPreview()
    {
        var session = CreatePurchasedSession(InvestmentType.SewingSideBusiness, 500, LocationId.Workshop, (NpcId.WorkshopBossAbuSamir, 20));
        session.Player.Skills.SetLevel(SkillId.Physical, 2);

        session.PreviewJob(JobType.WorkshopSewing).ActiveModifiers.Should().Contain(static modifier => modifier.Contains("2 LE", StringComparison.Ordinal));
    }

    [Test]
    public void CafeSupplyPartnership_ShouldIncreaseCafeStressRelief()
    {
        var session = CreatePurchasedSession(InvestmentType.CafeSupplyPartnership, 500, LocationId.Cafe, (NpcId.CafeOwnerNadia, 25));
        session.Player.Stats.SetStress(50);
        var result = session.TryPerformEntertainment(EntertainmentRegistry.AllActivities.Single(static activity => activity.Type == EntertainmentActivityType.Coffee));

        result.Should().BeTrue();
        session.Player.Stats.Stress.Should().Be(40);
    }

    private static GameSession CreatePurchasedSession(
        InvestmentType type,
        int money,
        LocationId location,
        params (NpcId Npc, int Trust)[] relationships)
    {
        var session = TestSessions.Create();
        session.Player.Stats.SetMoney(money);
        session.World.TravelTo(location);
        foreach (var (npc, trust) in relationships)
        {
            session.Relationships.SetNpcRelationship(npc, trust, 1);
        }

        if (type == InvestmentType.ScrapCollection)
        {
            session.Player.Skills.SetLevel(SkillId.StreetSmarts, 2);
        }
        else if (type == InvestmentType.HerbalRemedyTrade)
        {
            session.Player.Skills.SetLevel(SkillId.Medical, 2);
        }
        else if (type == InvestmentType.SewingSideBusiness)
        {
            session.Player.Skills.SetLevel(SkillId.Physical, 2);
        }

        var result = session.MakeInvestment(type);
        result.Success.Should().BeTrue(result.Message);
        return session;
    }

    private static SequenceRandom SuccessRandom()
    {
        return new SequenceRandom(doubleValues: [0.99, 0.99, 0.99, 0.99], intValues: [100]);
    }
}
