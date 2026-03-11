using Slums.Core.State;

namespace Slums.Application.Persistence;

public sealed record LoadedGameState(
    string Slot,
    string CheckpointName,
    DateTimeOffset CreatedUtc,
    DateTimeOffset LastPlayedUtc,
    string? LastKnot,
    GameState GameState);