using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Robotics;
using Slums.Core.Weather;

namespace Slums.Core.State;

public sealed partial class GameSession
{
    /// <summary>Hydrates this session through one validated snapshot restore boundary.</summary>
    /// <param name="restore">The persistence adapter that applies the captured state.</param>
    /// <returns>This hydrated session.</returns>
    public GameSession RestoreFromSnapshot(Action<GameSession> restore)
    {
        ArgumentNullException.ThrowIfNull(restore);
        restore(this);
        ValidateRestoredState();
        return this;
    }

    private void ValidateRestoredState()
    {
        if (Clock.Day < 1 || Clock.Hour is < 0 or > 23 || Clock.Minute is < 0 or > 59)
        {
            throw new InvalidOperationException("Restored session has an incoherent clock.");
        }

        if (Relationships.NpcRelationships.Count != Enum.GetValues<NpcId>().Length)
        {
            throw new InvalidOperationException("Restored session has an incomplete NPC relationship registry.");
        }

        if (JobProgress.Tracks.Count != Enum.GetValues<JobType>().Length)
        {
            throw new InvalidOperationException("Restored session has an incomplete job track registry.");
        }
    }

    internal void SetRunId(Guid runId)
    {
        RunId = runId;
    }

    internal void RestoreRunState(
        Guid runId,
        int daysSurvived,
        bool isGameOver,
        string? gameOverReason,
        EndingId? endingId,
        string? pendingEndingKnot,
        bool emergencySupportClaimed = false,
        EndingId? pendingEndingId = null,
        string? finalSacrifice = null)
    {
        SetRunId(runId);
        SetDaysSurvived(daysSurvived);
        IsGameOver = isGameOver;
        GameOverReason = string.IsNullOrWhiteSpace(gameOverReason) ? null : gameOverReason;
        EndingId = endingId;
        PendingEndingKnot = string.IsNullOrWhiteSpace(pendingEndingKnot) ? null : pendingEndingKnot;
        PendingEndingId = pendingEndingId;
        FinalSacrifice = string.IsNullOrWhiteSpace(finalSacrifice) ? null : finalSacrifice;
        _runState.EmergencySupportClaimed = emergencySupportClaimed;
    }

    internal void SetDaysSurvived(int daysSurvived)
    {
        DaysSurvived = Math.Max(0, daysSurvived);
    }

    internal void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted)
        => CrimeSessionService.SetCrimeCounters(this, totalCrimeEarnings, crimesCommitted);

    internal void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay)
        => CrimeSessionService.SetCrimeCounters(this, totalCrimeEarnings, crimesCommitted, lastCrimeDay);

    internal void RestoreCrimeState(int policePressure, int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay, bool hasCrimeCommittedToday)
        => CrimeSessionService.RestoreCrimeState(this, policePressure, totalCrimeEarnings, crimesCommitted, lastCrimeDay, hasCrimeCommittedToday);

    internal void RestoreWorkState(int totalHonestWorkEarnings, int honestShiftsCompleted, int lastHonestWorkDay, int lastPublicFacingWorkDay)
    {
        TotalHonestWorkEarnings = Math.Max(0, totalHonestWorkEarnings);
        HonestShiftsCompleted = Math.Max(0, honestShiftsCompleted);
        LastHonestWorkDay = Math.Max(0, lastHonestWorkDay);
        LastPublicFacingWorkDay = Math.Max(0, lastPublicFacingWorkDay);
    }

    internal void RestoreRentState(int unpaidRentDays, int accumulatedRentDebt, bool firstWarningGiven, bool finalWarningGiven, int graceDaysRemaining = 0)
    {
        _rentState.Restore(unpaidRentDays, accumulatedRentDebt, firstWarningGiven, finalWarningGiven);
        _rentState.RestoreGraceDays(graceDaysRemaining);
    }

    public void RecordEventHistory(string eventId, int count)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        _randomEventHistory[eventId] = Math.Max(0, count);
    }

    internal void RestoreNarrativeState(
        IEnumerable<string> storyFlags,
        IEnumerable<KeyValuePair<string, int>> randomEventHistory,
        IEnumerable<string> pendingNarrativeScenes)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        ArgumentNullException.ThrowIfNull(randomEventHistory);
        ArgumentNullException.ThrowIfNull(pendingNarrativeScenes);

        RestoreStoryFlags(storyFlags);
        _randomEventHistory.Clear();
        foreach (var pair in randomEventHistory)
        {
            RecordEventHistory(pair.Key, pair.Value);
        }

        _pendingNarrativeScenes.Clear();
        foreach (var scene in pendingNarrativeScenes.Where(static scene => !string.IsNullOrWhiteSpace(scene)))
        {
            _pendingNarrativeScenes.Enqueue(scene);
        }
    }

    internal void RestoreHouseholdAssetsState(
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings,
        IEnumerable<OwnedRobot>? robots = null,
        int robotParts = 0)
        => HouseholdAssetsService.Restore(this, pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings, robots, robotParts);

    internal void RestoreRamadanState(bool isActive, bool playerIsFasting, int daysFasting, int daysRemaining)
        => CalendarService.RestoreRamadanState(this, isActive, playerIsFasting, daysFasting, daysRemaining);

    internal void RestoreCommunityEventAttendance(
        int consecutiveSkips,
        int totalAttended,
        int lastAttendanceDay,
        IEnumerable<CommunityEventId> attendedThisWeek,
        int lastWeekResetDay,
        bool hasTeaCircleInvitation)
    {
        ArgumentNullException.ThrowIfNull(attendedThisWeek);

        EventAttendance.ConsecutiveSkips = consecutiveSkips;
        EventAttendance.TotalAttended = totalAttended;
        EventAttendance.LastAttendanceDay = lastAttendanceDay;
        EventAttendance.LastWeekResetDay = lastWeekResetDay;
        EventAttendance.HasTeaCircleInvitation = hasTeaCircleInvitation;

        foreach (var eventId in attendedThisWeek)
        {
            EventAttendance.AttendedThisWeek.Add(eventId);
        }
    }

    internal void RestoreWeather(WeatherType weatherType)
        => CalendarService.RestoreWeather(this, weatherType);

    public int GetEventCount(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return 0;
        }

        return _randomEventHistory.GetValueOrDefault(eventId);
    }

    internal void RestoreJobTrack(JobType jobType, int reliability, int shiftsCompleted, int lockoutUntilDay)
    {
        JobProgress.RestoreTrack(jobType, reliability, shiftsCompleted, lockoutUntilDay);
    }
}
