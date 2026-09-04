using Slums.Core.Clock;
using Slums.Core.Events;
using Slums.Core.State.DailyResolution;

namespace Slums.Core.State;

public sealed partial class GameSession
{
    private const int EndOfDayHour = 22;

    public void AdvanceTime(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        while (minutes > 0)
        {
            var currentMinutes = (Clock.Hour * 60) + Clock.Minute;
            const int endOfDayMinutes = EndOfDayHour * 60;

            if (currentMinutes >= endOfDayMinutes)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }

                continue;
            }

            var minutesUntilEndOfDay = endOfDayMinutes - currentMinutes;
            var minutesToAdvance = Math.Min(minutes, minutesUntilEndOfDay);

            Clock.AdvanceMinutes(minutesToAdvance);
            minutes -= minutesToAdvance;

            if (Clock.IsEndOfDay && !IsGameOver)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }
            }
        }
    }

    internal bool CanCompleteActivityToday(int durationMinutes)
    {
        return DailyActivityWindow.CanComplete(Clock, durationMinutes, EndOfDayHour);
    }

    /// <summary>Resolves the current day through the session-owned daily pipeline.</summary>
    public void EndDay(Random? random = null)
    {
        EndOfDayPipeline.Run(this, random);
    }
}
