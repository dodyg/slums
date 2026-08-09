using System.Numerics;

namespace Slums.Core.Randomness;

/// <summary>
/// Serializable internal state of a <see cref="GameRandom"/>.
/// </summary>
/// <param name="S0">xoshiro256** state word 0.</param>
/// <param name="S1">xoshiro256** state word 1.</param>
/// <param name="S2">xoshiro256** state word 2.</param>
/// <param name="S3">xoshiro256** state word 3.</param>
public sealed record GameRandomState(ulong S0, ulong S1, ulong S2, ulong S3);

/// <summary>
/// Deterministic pseudo-random generator used for gameplay randomness.
/// </summary>
/// <remarks>
/// Subclasses <see cref="Random"/> so it can be passed anywhere simulation code accepts
/// <see cref="Random"/> without changing call sites, while keeping its full internal state
/// capturable and restorable. Persisting <see cref="GameRandomState"/> in save files lets a
/// restored session continue the exact same random sequence as the uninterrupted run, so
/// weather, random events, economy, police pressure, phone messages, tips, investments, and
/// crime outcomes reproduce exactly.
/// </remarks>
public sealed class GameRandom : Random
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    /// <summary>Creates a generator seeded from <paramref name="seed"/>.</summary>
    public GameRandom(ulong seed)
    {
        var mix = seed;
        _s0 = SplitMix64(ref mix);
        _s1 = SplitMix64(ref mix);
        _s2 = SplitMix64(ref mix);
        _s3 = SplitMix64(ref mix);

        // xoshiro256** requires a non-zero state.
        if ((_s0 | _s1 | _s2 | _s3) == 0)
        {
            _s3 = 1;
        }
    }

    /// <summary>Creates a generator at an exact captured <paramref name="state"/>.</summary>
    public GameRandom(GameRandomState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _s0 = state.S0;
        _s1 = state.S1;
        _s2 = state.S2;
        _s3 = state.S3;
    }

    /// <summary>Captures the generator state for persistence.</summary>
    public GameRandomState CaptureState() => new(_s0, _s1, _s2, _s3);

    /// <summary>Restores the generator to an exact captured <paramref name="state"/>.</summary>
    public void RestoreState(GameRandomState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _s0 = state.S0;
        _s1 = state.S1;
        _s2 = state.S2;
        _s3 = state.S3;
    }

    /// <summary>Returns a value in [0, 1).</summary>
    protected override double Sample() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

    /// <summary>Returns a non-negative random integer in [0, int.MaxValue).</summary>
    public override int Next()
    {
        int result;
        do
        {
            result = (int)(NextUInt64() >> 33);
        }
        while (result == int.MaxValue);

        return result;
    }

    /// <summary>Returns a random integer in [0, <paramref name="maxValue"/>).</summary>
    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxValue);
        return (int)NextUInt64((ulong)maxValue);
    }

    /// <summary>Returns a random integer in [<paramref name="minValue"/>, <paramref name="maxValue"/>).</summary>
    public override int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

        var range = (long)maxValue - minValue;
        return range <= int.MaxValue
            ? minValue + (int)NextUInt64((ulong)range)
            : (int)((long)minValue + (long)(NextUInt64() % (ulong)range));
    }

    /// <summary>Returns a random floating-point number in [0, 1).</summary>
    public override double NextDouble() => Sample();

    /// <summary>Fills the buffer with random bytes.</summary>
    public override void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        NextBytes(buffer.AsSpan());
    }

    /// <summary>Fills the buffer with random bytes.</summary>
    public override void NextBytes(Span<byte> buffer)
    {
        for (var i = 0; i < buffer.Length;)
        {
            var value = NextUInt64();
            var remaining = buffer.Length - i;
            var count = Math.Min(8, remaining);
            for (var j = 0; j < count; j++)
            {
                buffer[i + j] = (byte)(value >> (j * 8));
            }

            i += count;
        }
    }

    private ulong NextUInt64()
    {
        var result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;

        var t = _s1 << 17;
        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;
        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return result;
    }

    private ulong NextUInt64(ulong maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfZero(maxExclusive);

        // Fast path for power-of-two bounds.
        if ((maxExclusive & (maxExclusive - 1)) == 0)
        {
            return NextUInt64() & (maxExclusive - 1);
        }

        // Rejection sampling avoids modulo bias.
        var threshold = (ulong)(-(long)maxExclusive) % maxExclusive;
        while (true)
        {
            var result = NextUInt64();
            if (result >= threshold)
            {
                return result % maxExclusive;
            }
        }
    }

    private static ulong SplitMix64(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15;

        var z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EB;
        return z ^ (z >> 31);
    }
}
