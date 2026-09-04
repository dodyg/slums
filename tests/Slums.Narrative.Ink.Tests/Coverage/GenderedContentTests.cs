using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class GenderedContentTests
{
    [Test]
    [Arguments("mona_default_4", "late pump hour")]
    [Arguments("fixer_double_life", "late roster")]
    [Arguments("safaa_default_1", "late loading roster")]
    [Arguments("iman_default_1", "dawn pump-hour shift")]
    [Arguments("crime_police_encounter", "questions are casual")]
    public async Task AuthoredScenes_DivergeByGender(string knotName, string expectedSharedPhrase)
    {
        var male = StoryTraversalHelper.ExplorePath(knotName, CreateState("male"));
        var female = StoryTraversalHelper.ExplorePath(knotName, CreateState("female"));

        string.Join(" ", male.Text).Should().Contain(expectedSharedPhrase);
        string.Join(" ", female.Text).Should().NotBe(string.Join(" ", male.Text));
    }

    private static NarrativeSceneState CreateState(string gender) => new(
        Money: 100,
        Health: 80,
        Energy: 70,
        Hunger: 60,
        Stress: 20,
        MotherHealth: 70,
        FoodStockpile: 3,
        Day: 5,
        Background: "SudaneseRefugee",
        Gender: gender);
}
