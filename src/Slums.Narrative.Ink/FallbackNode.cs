using Slums.Core.State;

namespace Slums.Narrative.Ink;

internal sealed record FallbackNode(string Id, Func<GameState, string> TextFactory, IReadOnlyList<FallbackChoice> Choices);