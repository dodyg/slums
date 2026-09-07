using System.Diagnostics;

namespace Slums.Core.Diagnostics;

[DebuggerDisplay("Mutation: {Record.Category}/{Record.Action} at {Record.Timestamp:o}")]
public sealed class GameMutationEventArgs(GameMutationRecord record) : EventArgs
{
    public GameMutationRecord Record { get; } = record;
}
