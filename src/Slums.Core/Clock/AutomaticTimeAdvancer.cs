namespace Slums.Core.Clock;

/// <summary>
/// Converts real elapsed time into in-game minutes while preserving partial progress between updates.
/// </summary>
public sealed class AutomaticTimeAdvancer
{
    private readonly TimeSpan _realTimePerGameMinute;
    private TimeSpan _bufferedRealTime;

    public AutomaticTimeAdvancer(TimeSpan realTimePerGameMinute)
    {
        if (realTimePerGameMinute <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(realTimePerGameMinute), "The interval must be greater than zero.");
        }

        _realTimePerGameMinute = realTimePerGameMinute;
    }

    /// <summary>
    /// Collects a real-time delta and returns how many in-game minutes should advance.
    /// </summary>
    /// <param name="delta">The real time elapsed since the previous update.</param>
    /// <returns>The number of whole in-game minutes ready to process.</returns>
    public int CollectElapsedMinutes(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "The elapsed time cannot be negative.");
        }

        _bufferedRealTime += delta;

        var elapsedMinutes = _bufferedRealTime.Ticks / _realTimePerGameMinute.Ticks;
        if (elapsedMinutes <= 0)
        {
            return 0;
        }

        _bufferedRealTime -= TimeSpan.FromTicks(elapsedMinutes * _realTimePerGameMinute.Ticks);
        return (int)elapsedMinutes;
    }
}
