using System.Diagnostics;

namespace Slums.Core.Diagnostics;

[DebuggerDisplay("Mutation: {Category}/{Action} at {Timestamp:o}")]
public sealed record GameMutationRecord(
    Guid RunId,
    DateTimeOffset Timestamp,
    string Category,
    string Action,
    IReadOnlyDictionary<string, object?> Before,
    IReadOnlyDictionary<string, object?> After,
    string Reason);
