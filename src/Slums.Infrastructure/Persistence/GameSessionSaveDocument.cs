namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionSaveDocument(
    int SaveVersion,
    DateTimeOffset CreatedUtc,
    DateTimeOffset LastPlayedUtc,
    string CheckpointName,
    GameSessionSnapshot SessionSnapshot,
    NarrativeProgressSnapshot NarrativeProgress);
