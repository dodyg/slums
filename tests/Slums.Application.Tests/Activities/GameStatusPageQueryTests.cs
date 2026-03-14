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

        pages.Select(static page => page.Title).Should().ContainInOrder("Survival", "Skills", "Network", "Investments", "Signals", "Progress");
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
    public void GetPages_ShouldExposeClinicStatus_OnSurvivalPage()
    {
        var query = new GameStatusPageQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Clinic);

        var pages = query.GetPages(GameStatusContext.Create(gameState));

        var survival = pages.Single(static page => page.Title == "Survival");
        survival.Lines.Should().Contain(static line => line.Contains("Clinic here: open today | visit 35 LE", StringComparison.Ordinal));
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
    }
}
