using Slums.Application.Randomness;

namespace Slums.Infrastructure.Randomness;

public sealed class SeededRandomSource : IRandomSource
{
    public SeededRandomSource()
        : this(Environment.TickCount)
    {
    }

    public SeededRandomSource(int seed)
    {
        SharedRandom = new Random(seed);
    }

    public Random SharedRandom { get; }
}