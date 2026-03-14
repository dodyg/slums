using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;

namespace Slums.Narrative.Ink.Tests.Helpers;

internal static class NarrativeOutcomeVerifier
{
    public static void VerifyOutcomesContainTag(IReadOnlyList<string> outcomeTags, string expectedTagPrefix, int expectedValue)
    {
        outcomeTags.Should().Contain(tag => tag.StartsWith(expectedTagPrefix, StringComparison.OrdinalIgnoreCase),
            $"expected {expectedTagPrefix} outcome to be present");
    }

    public static void VerifyOutcomesContainTag(IReadOnlyList<string> outcomeTags, string expectedTagPrefix)
    {
        outcomeTags.Should().Contain(tag => tag.StartsWith(expectedTagPrefix, StringComparison.OrdinalIgnoreCase),
            $"expected {expectedTagPrefix} outcome to be present");
    }

    public static void VerifyMoneyOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "MONEY", expectedValue);
    }

    public static void VerifyHealthOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "HEALTH", expectedValue);
    }

    public static void VerifyEnergyOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "ENERGY", expectedValue);
    }

    public static void VerifyHungerOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "HUNGER", expectedValue);
    }

    public static void VerifyStressOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "STRESS", expectedValue);
    }

    public static void VerifyMotherHealthOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "MOTHER_HEALTH", expectedValue);
    }

    public static void VerifyFoodOutcome(IReadOnlyList<string> outcomeTags, int expectedValue)
    {
        VerifyOutcomesContainTag(outcomeTags, "FOOD", expectedValue);
    }

    public static void VerifyFlagOutcome(IReadOnlyList<string> outcomeTags, string expectedFlag)
    {
        outcomeTags.Should().Contain(tag => tag.Equals($"FLAG:{expectedFlag}", StringComparison.OrdinalIgnoreCase),
            $"expected FLAG:{expectedFlag} outcome to be present");
    }

    public static void VerifyNpcTrustOutcome(IReadOnlyList<string> outcomeTags, string npcId, int expectedValue)
    {
        outcomeTags.Should().Contain(tag => tag.StartsWith($"NPC_TRUST:{npcId}", StringComparison.OrdinalIgnoreCase),
            $"expected NPC_TRUST:{npcId} outcome to be present");
    }

    public static void VerifyFactionRepOutcome(IReadOnlyList<string> outcomeTags, string factionId, int expectedValue)
    {
        outcomeTags.Should().Contain(tag => tag.StartsWith($"FACTION_REP:{factionId}", StringComparison.OrdinalIgnoreCase),
            $"expected FACTION_REP:{factionId} outcome to be present");
    }

    public static void VerifyMessageOutcome(IReadOnlyList<string> outcomeTags)
    {
        VerifyOutcomesContainTag(outcomeTags, "MESSAGE");
    }

    public static void VerifyPathHasText(StoryPathResult result, string expectedTextFragment)
    {
        result.Text.Should().Contain(text => text.Contains(expectedTextFragment, StringComparison.OrdinalIgnoreCase),
            $"expected path '{result.Path}' to contain text '{expectedTextFragment}'");
    }

    public static void VerifyPathHasChoices(StoryPathResult result, params string[] expectedChoiceFragments)
    {
        foreach (var fragment in expectedChoiceFragments)
        {
            result.Choices.Should().Contain(choice => choice.Contains(fragment, StringComparison.OrdinalIgnoreCase),
                $"expected path '{result.Path}' to have choice containing '{fragment}'");
        }
    }

    public static void VerifyPathHasNoChoices(StoryPathResult result)
    {
        result.Choices.Should().BeEmpty($"expected path '{result.Path}' to have no choices (ending)");
    }

    public static void VerifyAllPathsReachable(string knotPrefix, NarrativeSceneState? sceneState = null)
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix(knotPrefix);
        knots.Should().NotBeEmpty($"expected at least one knot with prefix '{knotPrefix}'");

        foreach (var knot in knots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, sceneState);
            result.Text.Should().NotBeEmpty($"knot '{knot}' should produce text");
        }
    }
}
