using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Simulation;

internal sealed class FullPlaythroughTests
{
    [Test]
    public async Task Playthrough_IntroToGame_CompletesWithoutError()
    {
        using var session = new GameStateBuilder()
            .WithBackground(BackgroundType.MedicalSchoolDropout)
            .WithMoney(50)
            .WithHealth(80)
            .WithEnergy(70)
            .Build();

        var sceneState = NarrativeSceneState.Create(session);
        var result = StoryTraversalHelper.ExplorePath("intro_medical", sceneState);

        result.Text.Should().NotBeEmpty("intro should produce narrative");
    }

    [Test]
    public async Task Playthrough_HonestWorkPath_ReachesStability()
    {
        using var session = new GameStateBuilder()
            .WithBackground(BackgroundType.MedicalSchoolDropout)
            .WithDaysSurvived(30)
            .WithMoney(250)
            .WithPolicePressure(10)
            .WithWorkCounters(400, 15, 30, 30)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.StabilityHonestWork, "honest work path should reach stability ending");

        var endingKnot = EndingService.GetInkKnot(session, EndingId.StabilityHonestWork);
        endingKnot.Should().NotBeNullOrEmpty("stability ending should have an Ink knot");

        var result = StoryTraversalHelper.ExplorePath(endingKnot!, NarrativeSceneState.Create(session));
        result.Text.Should().NotBeEmpty("ending knot should produce narrative");
    }

    [Test]
    public async Task Playthrough_CrimePath_CanReachKingpin()
    {
        using var session = new GameStateBuilder()
            .WithCrimeCounters(1100, 20)
            .WithFactionReputation(FactionId.ImbabaCrew, 55)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.CrimeKingpin, "high crime earnings should reach kingpin ending");

        var endingKnot = EndingService.GetInkKnot(session, ending!.Value);
        endingKnot.Should().NotBeNullOrEmpty("kingpin ending should have an Ink knot");
    }

    [Test]
    public async Task Playthrough_HighPressure_ReachesArrested()
    {
        using var session = new GameStateBuilder()
            .WithPolicePressure(100)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.Arrested, "max police pressure should trigger arrested ending");
    }

    [Test]
    public async Task Playthrough_MotherDeath_TriggeredByZeroHealth()
    {
        using var session = new GameStateBuilder()
            .WithMotherHealth(0)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.MotherDied, "zero mother health should trigger mother death ending");
    }

    [Test]
    public async Task Playthrough_Exhaustion_TriggeredByZeroHealth()
    {
        using var session = new GameStateBuilder()
            .WithHealth(0)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.CollapseFromExhaustion, "zero health should trigger exhaustion ending");
    }

    [Test]
    public async Task Playthrough_NetworkShelter_RequiresHighTrust()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(150)
            .WithNpcTrust(NpcId.NeighborMona, 45)
            .WithNpcTrust(NpcId.NurseSalma, 45)
            .WithNpcTrust(NpcId.CafeOwnerNadia, 40)
            .WithNpcTrust(NpcId.FenceHanan, 40)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.NetworkShelter, "high NPC trust should trigger network shelter ending");
    }

    [Test]
    public async Task Playthrough_AllBackgrounds_HaveWorkingIntros()
    {
        var backgrounds = new[]
        {
            BackgroundType.MedicalSchoolDropout,
            BackgroundType.ReleasedPoliticalPrisoner,
            BackgroundType.SudaneseRefugee
        };

        var introKnots = new[]
        {
            "intro_medical",
            "intro_prisoner",
            "intro_sudanese"
        };

        for (var i = 0; i < backgrounds.Length; i++)
        {
            using var session = GameStateBuilder.BuildForBackground(backgrounds[i]);
            var sceneState = NarrativeSceneState.Create(session);
            var result = StoryTraversalHelper.ExplorePath(introKnots[i], sceneState);

            result.Text.Should().NotBeEmpty($"intro '{introKnots[i]}' should work for background '{backgrounds[i]}'");
            result.Choices.Should().NotBeEmpty($"intro '{introKnots[i]}' should offer choices");
        }
    }

    [Test]
    public async Task Playthrough_CrimeLeaving_RequiresRecentWorkAndOldCrimes()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithPolicePressure(30)
            .WithCrimeCounters(300, 5, lastCrimeDay: 25)
            .WithWorkCounters(220, 6, 30, 30)
            .OnDay(30)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.LeavingCrime, "transition from crime to work should trigger leaving crime ending");
    }

    [Test]
    public async Task Playthrough_BuriedByHeat_RequiresSustainedCriminalActivity()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithCrimeCounters(500, 7)
            .WithPolicePressure(90)
            .WithStress(75)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.BuriedByHeat, "sustained criminal heat should trigger buried by heat ending");
    }

    [Test]
    public async Task Playthrough_LuxorDream_RequiresEscape()
    {
        using var session = new GameStateBuilder()
            .WithDaysSurvived(30)
            .WithMoney(550)
            .WithCrimeCounters(0, 2)
            .WithMotherHealth(70)
            .Build();

        var ending = EndingService.CheckEndings(session);
        ending.Should().Be(EndingId.QuitTheLuxorDream, "saving enough money with low crime should trigger Luxor dream");
    }
}
