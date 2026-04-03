using FluentAssertions;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Simulation;

internal sealed class EndingConditionSimulationTests
{
    [Test]
    public async Task Ending_Priority_MotherDeathComesFirst()
    {
        using var session = new GameStateBuilder()
            .WithMotherHealth(0)
            .WithHealth(0)
            .WithMoney(0)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.MotherDied, "mother death should have highest priority");
    }

    [Test]
    public async Task Ending_Priority_HealthZeroFoldsIntoDestitution()
    {
        using var session = new GameStateBuilder()
            .WithHealth(0)
            .WithMoney(0)
            .WithHunger(10)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.Destitution, "health at zero should trigger destitution");
    }

    [Test]
    public async Task Ending_Priority_ArrestedBeforeOthers()
    {
        using var session = new GameStateBuilder()
            .WithPolicePressure(100)
            .WithMoney(500)
            .WithDaysSurvived(30)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.Arrested, "arrested should trigger at max pressure");
    }

    [Test]
    public async Task Ending_NoEnding_WhenConditionsNotMet()
    {
        using var session = new GameStateBuilder()
            .WithMotherHealth(50)
            .WithHealth(60)
            .WithMoney(100)
            .WithPolicePressure(20)
            .WithDaysSurvived(10)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("no ending should trigger when conditions are not met");
    }

    [Test]
    public async Task Ending_CrimeKingpin_RequiresHighEarnings()
    {
        using var session = new GameStateBuilder()
            .WithCrimeCounters(500, 10)
            .WithFactionReputation(FactionId.ImbabaCrew, 30)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("intermediate crime earnings should not trigger kingpin");

        using var session2 = new GameStateBuilder()
            .WithCrimeCounters(1100, 20)
            .WithFactionReputation(FactionId.ImbabaCrew, 55)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.CrimeKingpin, "high crime earnings should trigger kingpin");
    }

    [Test]
    public async Task Ending_NetworkShelter_RequiresMultipleHighTrust()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(100)
            .WithNpcTrust(NpcId.NeighborMona, 20)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("low trust with one NPC should not trigger network shelter");

        using var session2 = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(150)
            .WithNpcTrust(NpcId.NeighborMona, 45)
            .WithNpcTrust(NpcId.NurseSalma, 45)
            .WithNpcTrust(NpcId.CafeOwnerNadia, 40)
            .WithNpcTrust(NpcId.FenceHanan, 40)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.NetworkShelter, "high trust with multiple NPCs should trigger network shelter");
    }

    [Test]
    public async Task Ending_StabilityHonestWork_AllowsFormerCriminals()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithPolicePressure(30)
            .WithCrimeCounters(300, 5, lastCrimeDay: 10)
            .WithWorkCounters(100, 2, 15, 15)
            .OnDay(30)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("old crimes without recent work should not trigger stability");

        using var session2 = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithPolicePressure(30)
            .WithCrimeCounters(300, 5, lastCrimeDay: 25)
            .WithWorkCounters(220, 6, 30, 30)
            .OnDay(30)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.StabilityHonestWork, "recent work with aging crimes should trigger stability");
    }

    [Test]
    public async Task Ending_Arrested_IncludesBuriedByHeatScenario()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithCrimeCounters(500, 7)
            .WithPolicePressure(70)
            .WithStress(50)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("moderate pressure should not trigger arrested");

        using var session2 = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithCrimeCounters(500, 7)
            .WithPolicePressure(90)
            .WithStress(75)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.Arrested, "high sustained pressure should trigger arrested");
    }

    [Test]
    public async Task Ending_LuxorDream_RequiresLowCrime()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(550)
            .WithCrimeCounters(400, 8)
            .WithMotherHealth(70)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("high crime should prevent Luxor dream");

        using var session2 = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(550)
            .WithCrimeCounters(0, 2)
            .WithMotherHealth(70)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.QuitTheLuxorDream, "low crime with savings should trigger Luxor dream");
    }

    [Test]
    public async Task Ending_Stability_RequiresWork()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(250)
            .WithPolicePressure(10)
            .WithWorkCounters(100, 5, 20, 20)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().BeNull("insufficient work should not trigger stability");

        using var session2 = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(250)
            .WithPolicePressure(10)
            .WithWorkCounters(400, 15, 30, 30)
            .Build();

        var ending2 = EndingService.CheckEndings(session2);
        ending2.Should().Be(EndingId.StabilityHonestWork, "sufficient work should trigger stability");
    }

    [Test]
    public async Task Ending_GetInkKnot_ReturnsValidKnots()
    {
        var endings = new[]
        {
            EndingId.MotherDied,
            EndingId.Destitution,
            EndingId.Arrested,
            EndingId.Eviction,
            EndingId.NetworkShelter,
            EndingId.QuitTheLuxorDream,
            EndingId.StabilityHonestWork,
            EndingId.CrimeKingpin
        };

        using var session = new GameStateBuilder().Build();

        foreach (var endingId in endings)
        {
            var knot = EndingService.GetInkKnot(session, endingId);
            knot.Should().NotBeNullOrEmpty($"ending {endingId} should have an Ink knot");
            knot.Should().StartWith("ending_", $"ending knot should start with 'ending_'");
        }
    }
}
