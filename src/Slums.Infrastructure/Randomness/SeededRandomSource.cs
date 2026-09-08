using Slums.Application.Randomness;
using Slums.Core.Randomness;
using Microsoft.Extensions.Logging;
using Slums.Core.Diagnostics;

namespace Slums.Infrastructure.Randomness;

/// <summary>
/// Seeds the game's shared random source for a new run. The source is a <see cref="GameRandom"/>
/// so that every new session's randomness is capturable for deterministic save/load.
/// </summary>
public sealed class SeededRandomSource : IRandomSource
{
    private readonly ILogger<SeededRandomSource>? _logger;

    public SeededRandomSource()
        : this(NewEntropySeed(), null)
    {
    }

    public SeededRandomSource(int seed)
        : this(seed, null)
    {
    }

    public SeededRandomSource(ILogger<SeededRandomSource> logger)
        : this(NewEntropySeed(), logger)
    {
    }

    private SeededRandomSource(int seed, ILogger<SeededRandomSource>? logger)
    {
        _logger = logger;
        Seed = seed;
        SharedRandom = new GameRandom(unchecked((ulong)seed));
        LogSeedCreated(_logger, seed);
    }

    public int Seed { get; }

    public Random SharedRandom { get; }

    /// <summary>Draws a process-entropy seed without touching System.Random directly.</summary>
    private static int NewEntropySeed()
    {
        return Guid.NewGuid().GetHashCode();
    }

    private static readonly Action<ILogger, int, Exception?> LogSeedCreatedDelegate =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(global::Slums.Core.Diagnostics.LogEvents.RandomSeedCreated, "RandomSeedCreated"), "Created gameplay random source with seed {Seed}.");

    private static void LogSeedCreated(ILogger<SeededRandomSource>? logger, int seed)
    {
        if (logger is not null)
        {
            LogSeedCreatedDelegate(logger, seed, null);
        }
    }
}
