using Slums.Application.Narrative;
using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed record TalkSceneRequest(
    NpcId NpcId,
    string KnotName,
    NarrativeSceneState SceneState,
    string VariantId = "",
    string ConversationKnot = "");
