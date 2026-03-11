namespace Slums.Application.Persistence;

public sealed record SaveSlotMetadata(string Slot, string CheckpointName, DateTimeOffset LastPlayedUtc);