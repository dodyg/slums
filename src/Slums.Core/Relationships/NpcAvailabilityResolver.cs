using Slums.Core.Clock;
using Slums.Core.World;

namespace Slums.Core.Relationships;

public static class NpcAvailabilityResolver
{
    public static NpcAvailability Resolve(
        NpcId npc,
        GameClock clock,
        LocationId currentLocation,
        IReadOnlyList<NpcScheduleDefinition> schedules,
        IReadOnlySet<string>? activeNewsIds = null)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(schedules);

        var minute = (clock.Hour * 60) + clock.Minute;
        var matching = schedules
            .Where(schedule => schedule.Npc == npc)
            .Where(schedule => schedule.Days.Contains(clock.DayOfWeek))
            .Where(schedule => minute >= schedule.StartMinute && minute < schedule.EndMinute)
            .ToArray();

        var activeSchedule = matching.FirstOrDefault(schedule =>
            schedule.ConditionId is null || activeNewsIds?.Contains(schedule.ConditionId) == true);
        if (activeSchedule is not null)
        {
            return new NpcAvailability
            {
                Npc = npc,
                IsAvailable = activeSchedule.Location == currentLocation,
                Location = activeSchedule.Location,
                Reason = activeSchedule.Location == currentLocation
                    ? string.Empty
                    : $"{NpcRegistry.GetName(npc)} is at {activeSchedule.Location.Value}."
            };
        }

        var daySchedule = schedules.FirstOrDefault(schedule => schedule.Npc == npc && schedule.Days.Contains(clock.DayOfWeek));
        return new NpcAvailability
        {
            Npc = npc,
            IsAvailable = false,
            Location = daySchedule?.Location,
            Reason = daySchedule?.AbsenceReason ?? $"{NpcRegistry.GetName(npc)} is not available today."
        };
    }

    public static IReadOnlyList<NpcAvailability> ResolveAll(
        GameClock clock,
        LocationId currentLocation,
        IReadOnlyList<NpcScheduleDefinition> schedules,
        IReadOnlySet<string>? activeNewsIds = null)
    {
        return Enum.GetValues<NpcId>()
            .Select(npc => Resolve(npc, clock, currentLocation, schedules, activeNewsIds))
            .ToArray();
    }
}
