namespace Slums.Infrastructure.Persistence;

public sealed record SaveEnvelope(
    int SaveVersion,
    Guid RunId,
    DateTimeOffset CreatedUtc,
    DateTimeOffset LastPlayedUtc,
    string CheckpointName,
    GameStateDto GameState,
    NarrativeStateDto NarrativeState);