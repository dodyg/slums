using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class EventScenePathTests
{
    [Test]
    public async Task Event_KnotsExist()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("event_");
        knots.Should().NotBeEmpty("random events should have scene knots");
    }

    [Test]
    public async Task Event_MotherScenes_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var motherEvents = allKnots.Where(k => k.Contains("mother", StringComparison.OrdinalIgnoreCase) && k.StartsWith("event_", StringComparison.OrdinalIgnoreCase)).ToList();

        motherEvents.Should().NotBeEmpty("mother-related event scenes should exist");
    }

    [Test]
    public async Task Event_NeighborScenes_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var neighborEvents = allKnots.Where(k => k.Contains("neighbor", StringComparison.OrdinalIgnoreCase) && k.StartsWith("event_", StringComparison.OrdinalIgnoreCase)).ToList();

        neighborEvents.Should().NotBeEmpty("neighbor-related event scenes should exist");
    }

    [Test]
    public async Task Event_PublicWorkHeat_Exists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("event_public_work_heat", "public work heat event should exist");
    }

    [Test]
    public async Task Event_PublicWorkHeat_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("event_public_work_heat", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("event_public_work_heat should produce narrative text");
    }

    [Test]
    public async Task Event_MotherWrongMoney_Exists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("event_mother_wrong_money", "mother wrong money event should exist");
    }

    [Test]
    public async Task Event_MotherWrongMoney_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("event_mother_wrong_money", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("event_mother_wrong_money should produce narrative text");
    }

    [Test]
    public async Task Event_NeighborWatch_Exists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("event_neighbor_watch", "neighbor watch event should exist");
    }

    [Test]
    public async Task Event_NeighborWatch_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("event_neighbor_watch", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("event_neighbor_watch should produce narrative text");
    }

    [Test]
    public async Task Event_ClinicFirstVisit_Exists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("mother_clinic_first_visit", "mother clinic first visit event should exist");
    }

    [Test]
    public async Task Event_ClinicFirstVisit_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("mother_clinic_first_visit", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("mother_clinic_first_visit should produce narrative text");
    }

    [Test]
    public async Task Event_BackgroundMedicalClinic_Exists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("background_medical_clinic", "medical background clinic event should exist");
    }

    [Test]
    public async Task Event_BackgroundMedicalClinic_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("background_medical_clinic", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("background_medical_clinic should produce narrative text");
    }

    [Test]
    public async Task Event_AllEventKnots_ProduceText()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var eventKnots = allKnots.Where(k => k.StartsWith("event_", StringComparison.OrdinalIgnoreCase)).ToList();

        eventKnots.Should().NotBeEmpty("there should be event knots");

        foreach (var knot in eventKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            result.Text.Should().NotBeEmpty($"event knot '{knot}' should produce text");
        }
    }

    [Test]
    public async Task Event_CrimeRelatedEvents_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var crimeEvents = allKnots.Where(k => k.StartsWith("crime_", StringComparison.OrdinalIgnoreCase) && k.Contains("hanan", StringComparison.OrdinalIgnoreCase)).ToList();

        crimeEvents.Should().NotBeEmpty("Hanan crime-related scenes should exist");
    }

    private static NarrativeSceneState CreateDefaultSceneState()
    {
        using var session = new GameStateBuilder().Build();
        return NarrativeSceneState.Create(session);
    }
}
