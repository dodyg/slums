using Slums.Application.Narrative;

namespace Slums.Application.Activities;

public sealed record TalkSceneRequest(string KnotName, NarrativeSceneState SceneState);
