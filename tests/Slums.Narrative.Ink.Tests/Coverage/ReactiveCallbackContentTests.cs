using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class ReactiveCallbackContentTests
{
    [Test]
    [Arguments("NeighborMona", "helped", "bread you carried upstairs")]
    [Arguments("NurseSalma", "debt_warm", "debt without turning it into a sermon")]
    [Arguments("NeighborMona", "recent_refusal", "refused favor remains")]
    [Arguments("NeighborMona", "heat", "protection money and the police attention")]
    public async Task RecurringConversation_UsesReactiveContextCallback(string npc, string context, string expectedText)
    {
        var state = new NarrativeSceneState(100, 80, 70, 60, 20, 70, 3, 5, "SudaneseRefugee", "female")
        {
            ConversationNpc = npc,
            ConversationContext = context
        };

        var result = StoryTraversalHelper.ExplorePath("recurring_conversation", state);

        string.Join(" ", result.Text).Should().Contain(expectedText);
    }
}
