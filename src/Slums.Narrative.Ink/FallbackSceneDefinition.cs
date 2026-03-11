namespace Slums.Narrative.Ink;

internal sealed record FallbackSceneDefinition(string StartNodeId, IReadOnlyDictionary<string, FallbackNode> Nodes);