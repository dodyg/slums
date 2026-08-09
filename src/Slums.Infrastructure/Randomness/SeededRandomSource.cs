using Slums.Application.Randomness;
using Slums.Core.Randomness;

namespace Slums.Infrastructure.Randomness;

/// <summary>
/// Seeds the game's shared random source for a new run. The source is a <see cref="GameRandom"/>
/// so that every new session's randomness is capturable for deterministic save/load.
/// </summary>
public sealed class SeededRandomSource : IRandomSource
{
    public SeededRandomSource()
        : this(Environment.TickCount)
    {
    }

    public SeededRandomSource(int seed)
    {
        SharedRandom = new GameRandom(unchecked((ulong)seed));
    }

    public Random SharedRandom { get; }
}
