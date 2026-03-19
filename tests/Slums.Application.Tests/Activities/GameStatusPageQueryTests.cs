using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class GameStatusPageQueryTests
{
    [Test]
    public void GetPages_ShouldExposeExpectedPageSet()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        pages.Select(static page => page.Title).Should().ContainInOrder("Household", "City", "Debt", "Skills", "Network", "Heat", "Investments", "Signals", "Progress");
    }

    [Test]
    public void GetPages_ShouldExposeSkillAndNetworkDetails()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        gameState.Player.Skills.SetLevel(SkillId.Persuasion, 4);
        gameState.Relationships.SetFactionStanding(FactionId.ImbabaCrew, 22);
        gameState.Relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 14, 2);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var skills = pages.Single(static page => page.Title == "Skills");
        var network = pages.Single(static page => page.Title == "Network");

        skills.Lines.Should().Contain(static line => line.Contains("Medical: 3", StringComparison.Ordinal));
        skills.Lines.Should().Contain(static line => line.Contains("Persuasion: 4", StringComparison.Ordinal));
        network.Lines.Should().Contain(static line => line.Contains("Imbaba Crew: 22", StringComparison.Ordinal));
        network.Lines.Should().Contain(static line => line.Contains("Umm Karim: trust 14", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeDebtAndRelationshipMemorySignals()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.RestoreRentState(unpaidRentDays: 5, accumulatedRentDebt: 100, firstWarningGiven: true, finalWarningGiven: true);
        gameState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 2);
        gameState.Relationships.SetNpcRelationshipMemory(
            NpcId.NurseSalma,
            lastFavorDay: 2,
            lastRefusalDay: 0,
            hasUnpaidDebt: true,
            wasEmbarrassed: false,
            wasHelped: true,
            recentContactCount: 2);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var debt = pages.Single(static page => page.Title == "Debt");
        var network = pages.Single(static page => page.Title == "Network");

        debt.Lines.Should().Contain(static line => line.Contains("final warning", StringComparison.OrdinalIgnoreCase));
        debt.Lines.Should().Contain(static line => line.Contains("Rent is overdue", StringComparison.Ordinal));
        debt.Lines.Should().Contain(static line => line.Contains("Nurse Salma", StringComparison.Ordinal));
        network.Lines.Should().Contain(static line => line.Contains("you owe them", StringComparison.Ordinal));
        network.Lines.Should().Contain(static line => line.Contains("helped you", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldAvoidRepeatingOverviewFields_OnStatusPages()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.SetPolicePressure(65);
        gameState.RestoreRentState(unpaidRentDays: 3, accumulatedRentDebt: 60, firstWarningGiven: true, finalWarningGiven: false);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var lines = pages.SelectMany(static page => page.Lines).ToArray();
        lines.Should().NotContain(static line => line.StartsWith("Day ", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Location:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("District:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Money:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Police pressure:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Rent debt:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Local prices:", StringComparison.Ordinal));
        lines.Should().NotContain(static line => line.StartsWith("Clinic here:", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeDailyCityBulletins()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Imbaba, ConditionId = "imbaba_market_crackdown" },
            new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_checkpoint_sweep" }
        ]);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var city = pages.Single(static page => page.Title == "City");

        city.Lines.Should().Contain(static line => line.Contains("Dokki: Checkpoint Sweep", StringComparison.Ordinal));
        city.Lines.Should().NotContain(static line => line.Contains("Imbaba: Market Crackdown", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeHouseholdAssets_OnHouseholdPage()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.TryTriggerStreetCatEncounter(1);
        gameState.Player.HouseholdAssets.BuyFishTank(1, 1);
        gameState.Player.HouseholdAssets.BuyPlant(PlantType.Chamomile, 1, 1);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var household = pages.Single(static page => page.Title == "Household");
        household.Lines.Should().Contain(static line => line.Contains("Cats: 0/3", StringComparison.Ordinal));
        household.Lines.Should().Contain(static line => line.Contains("Fish tank: yes", StringComparison.Ordinal));
        household.Lines.Should().Contain(static line => line.Contains("Plants: 1/10", StringComparison.Ordinal));
        household.Lines.Should().Contain(static line => line.Contains("Street cat encounter: available", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeProgressTrajectoryHints()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.SetDaysSurvived(30);
        gameState.SetCrimeCounters(totalCrimeEarnings: 1050, crimesCommitted: 2);
        gameState.SetPolicePressure(10);
        gameState.Relationships.SetFactionStanding(FactionId.ImbabaCrew, 55);
        gameState.Player.Stats.ModifyMoney(600);
        gameState.Player.Household.UpdateMotherHealth(10);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var progress = pages.Single(static page => page.Title == "Progress");
        progress.Lines.Should().Contain(static line => line.Contains("Crime earnings: 1050 LE", StringComparison.Ordinal));
        progress.Lines.Should().Contain(static line => line.Contains("Luxor route conditions are currently strong", StringComparison.Ordinal));
        progress.Lines.Should().Contain(static line => line.Contains("Crime-kingpin route is within reach", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldIncludeSignalsPage_WithActiveNarrativeHooks()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        gameState.SetPolicePressure(65);
        gameState.SetWorkCounters(0, 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);
        gameState.SetCrimeCounters(150, 2, lastCrimeDay: 1);
        gameState.Player.Household.SetMotherHealth(55);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 18, 1);
        gameState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var signalsPage = pages.Single(static page => page.Title == "Signals");
        signalsPage.Lines.Should().Contain(static text => text.Contains("Public-facing work", StringComparison.Ordinal));
        signalsPage.Lines.Should().Contain(static text => text.Contains("tense reaction to sudden money", StringComparison.Ordinal));
        signalsPage.Lines.Should().Contain(static text => text.Contains("Mona", StringComparison.Ordinal));
        signalsPage.Lines.Should().Contain(static text => text.Contains("clinic reflection", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeHeatPagePressureAndDistrictSignals()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Square);
        gameState.SetPolicePressure(65);
        gameState.SetCrimeCounters(totalCrimeEarnings: 120, crimesCommitted: 2, lastCrimeDay: 1);
        gameState.Relationships.SetFactionStanding(FactionId.DokkiThugs, 14);
        gameState.SetWorkCounters(0, 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var heat = pages.Single(static page => page.Title == "Heat");
        heat.Lines.Should().Contain(static line => line.Contains("materially raising crime risk", StringComparison.Ordinal));
        heat.Lines.Should().Contain(static line => line.Contains("Dokki Thugs 14", StringComparison.Ordinal));
        heat.Lines.Should().Contain(static line => line.Contains("extra suspicion", StringComparison.Ordinal));
    }

    [Test]
    public void GetPages_ShouldExposeInvestmentProgress_OnInvestmentPage()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.RestoreInvestmentState(
        [
            new InvestmentSnapshot(InvestmentType.FoulCart, 150, 8, 12, 2, false)
        ],
        totalInvestmentEarnings: 19);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var investments = pages.Single(static page => page.Title == "Investments");
        investments.Lines.Should().Contain(static line => line.Contains("Active investments: 1", StringComparison.Ordinal));
        investments.Lines.Should().Contain(static line => line.Contains("Total investment earnings: 19 LE", StringComparison.Ordinal));
        investments.Lines.Should().Contain(static line => line.Contains("Foul Cart Partnership", StringComparison.Ordinal));
        investments.Lines.Should().Contain(static line => line.Contains("fail 1%", StringComparison.Ordinal));
    }
}
