using System.Diagnostics;

namespace Slums.Core.Diagnostics;

[DebuggerDisplay("Mutation: {Record.Category}/{Record.Action} at {Record.Timestamp:o}")]
public sealed class GameMutationEventArgs(GameMutationRecord record) : EventArgs
{
    public GameMutationRecord Record { get; } = record;
}

[DebuggerDisplay("Mutation: {Category}/{Action} at {Timestamp:o}")]
public sealed record GameMutationRecord(
    Guid RunId,
    DateTimeOffset Timestamp,
    string Category,
    string Action,
    IReadOnlyDictionary<string, object?> Before,
    IReadOnlyDictionary<string, object?> After,
    string Reason);
