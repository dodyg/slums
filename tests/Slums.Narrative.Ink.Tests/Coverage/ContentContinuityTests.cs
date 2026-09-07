using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class ContentContinuityTests
{
    [Test]
    public async Task RainLeakWithCurtain_UsesKhartoumOnlyForSudaneseBackground()
    {
        var sudanese = StoryTraversalHelper.ExplorePath(
            "event_rain_leak_with_curtain", CreateState(background: "SudaneseRefugee", gender: "female"));
        var medical = StoryTraversalHelper.ExplorePath(
            "event_rain_leak_with_curtain", CreateState(background: "MedicalSchoolDropout", gender: "female"));

        string.Join(" ", sudanese.Text).Should().Contain("Khartoum");
        string.Join(" ", medical.Text).Should().NotContain("Khartoum");
        string.Join(" ", medical.Text).Should().Contain("roof will hold this year");
    }

    [Test]
    public async Task Khamsin_HeadCoveringMatchesGender()
    {
        var male = StoryTraversalHelper.ExploreAllChoices("event_khamsin", CreateState("MedicalSchoolDropout", "male"));
        var female = StoryTraversalHelper.ExploreAllChoices("event_khamsin", CreateState("MedicalSchoolDropout", "female"));

        string.Join(" ", male.SelectMany(result => result.Text)).Should().Contain("shemagh");
        string.Join(" ", male.SelectMany(result => result.Text)).Should().NotContain("headscarf");
        string.Join(" ", female.SelectMany(result => result.Text)).Should().Contain("headscarf");
        string.Join(" ", female.SelectMany(result => result.Text)).Should().NotContain("shemagh");
    }

    [Test]
    public async Task MulidCelebration_DanceCircleMatchesGender()
    {
        var male = StoryTraversalHelper.ExploreAllChoices("event_mulid", CreateState("MedicalSchoolDropout", "male"));
        var female = StoryTraversalHelper.ExploreAllChoices("event_mulid", CreateState("MedicalSchoolDropout", "female"));

        string.Join(" ", male.SelectMany(result => result.Text)).Should().Contain("ring of clapping men");
        string.Join(" ", male.SelectMany(result => result.Text)).Should().NotContain("circle of women");
        string.Join(" ", female.SelectMany(result => result.Text)).Should().Contain("circle of women");
    }

    [Test]
    public async Task FridayPrisoner_PrayerRowMatchesGender()
    {
        var male = StoryTraversalHelper.ExplorePath("event_friday_prisoner", CreateState("ReleasedPoliticalPrisoner", "male"));
        var female = StoryTraversalHelper.ExplorePath("event_friday_prisoner", CreateState("ReleasedPoliticalPrisoner", "female"));

        string.Join(" ", male.Text).Should().Contain("back row of the men");
        string.Join(" ", female.Text).Should().Contain("back row of the women");
    }

    [Test]
    public async Task WinterFirstRain_MessageDoesNotReferenceMissingChild()
    {
        var result = StoryTraversalHelper.ExplorePath("event_winter_first_rain", CreateState("MedicalSchoolDropout", "female"));

        string.Join(" ", result.Text).Should().NotContain("child");
        result.OutcomeTags.Where(tag => tag.Contains("child in the lane", StringComparison.Ordinal)).Should().BeEmpty();
    }

    [Test]
    public async Task RecurringConversation_SuppressesGenericLineWhenNpcSpecificLineExists()
    {
        var monaHelped = StoryTraversalHelper.ExplorePath(
            "recurring_conversation", CreateState("MedicalSchoolDropout", "female") with
            {
                ConversationNpc = "NeighborMona",
                ConversationContext = "helped"
            });
        var salmaDebtWarm = StoryTraversalHelper.ExplorePath(
            "recurring_conversation", CreateState("MedicalSchoolDropout", "female") with
            {
                ConversationNpc = "NurseSalma",
                ConversationContext = "debt_warm"
            });

        string.Join(" ", monaHelped.Text).Should().Contain("bread you carried upstairs");
        string.Join(" ", monaHelped.Text).Should().NotContain("Something you did remains in the room");
        string.Join(" ", salmaDebtWarm.Text).Should().Contain("debt without turning it into a sermon");
        string.Join(" ", salmaDebtWarm.Text).Should().NotContain("Gratitude and obligation have become difficult");
    }

    [Test]
    [Arguments("ending_stability_prisoner")]
    [Arguments("ending_stability_sudanese")]
    [Arguments("ending_luxor")]
    [Arguments("ending_luxor_prisoner")]
    [Arguments("ending_network_shelter_mona")]
    [Arguments("ending_network_shelter_hanan")]
    public async Task GoodEndingVariants_ReachCrisisReflection(string knotName)
    {
        var result = StoryTraversalHelper.ExplorePath(knotName, CreateState("ReleasedPoliticalPrisoner", "male"));

        string.Join(" ", result.Text).Should().Contain("cooperative's crisis remains unresolved");
    }

    [Test]
    public async Task FixerFirstContact_DoesNotAddressPlayerAsWoman()
    {
        var male = StoryTraversalHelper.ExplorePath("fixer_first_contact", CreateState("SudaneseRefugee", "male"));
        var female = StoryTraversalHelper.ExplorePath("fixer_first_contact", CreateState("SudaneseRefugee", "female"));

        string.Join(" ", male.Text).Should().Contain("errands for people");
        string.Join(" ", female.Text).Should().Contain("errands for people");
    }

    private static NarrativeSceneState CreateState(string background, string gender) => new(
        Money: 100,
        Health: 80,
        Energy: 70,
        Hunger: 60,
        Stress: 20,
        MotherHealth: 70,
        FoodStockpile: 3,
        Day: 5,
        Background: background,
        Gender: gender);
}
