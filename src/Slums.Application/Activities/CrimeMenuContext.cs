using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record CrimeMenuContext(
    Location? Location,
    PlayerCharacter Player,
    RelationshipState Relationships,
    JobProgressState JobProgress,
    int PolicePressure,
    int TotalCrimeEarnings,
    int CrimesCommitted,
    IReadOnlyList<CrimeMenuOptionContext> Options,
    IReadOnlySet<string> StoryFlags)
{
    public static CrimeMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var location = gameSession.World.GetCurrentLocation();
        CrimeMenuOptionContext[] options = [];
        if (location is not null && location.HasCrimeOpportunities)
        {
            var availableByType = gameSession
                .GetAvailableCrimes()
                .GroupBy(static attempt => attempt.Type)
                .ToDictionary(static group => group.Key, static group => group.Last());

            options = CrimeRegistry
                .GetCrimeOpportunityStatuses(location, gameSession.Relationships)
                .Select(status => CreateOption(gameSession, availableByType, status))
                .ToArray();
        }

        return new CrimeMenuContext(
            location,
            gameSession.Player,
            gameSession.Relationships,
            gameSession.JobProgress,
            gameSession.PolicePressure,
            gameSession.TotalCrimeEarnings,
            gameSession.CrimesCommitted,
            options,
            gameSession.StoryFlags.ToHashSet(StringComparer.Ordinal));
    }

    public bool HasStoryFlag(string flag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flag);
        return StoryFlags.Contains(flag);
    }

    private static CrimeMenuOptionContext CreateOption(
        GameSession gameSession,
        Dictionary<CrimeType, CrimeAttempt> availableByType,
        CrimeOpportunityStatus status)
    {
        var attempt = availableByType.TryGetValue(status.Attempt.Type, out var availableAttempt)
            ? availableAttempt
            : status.Attempt;

        return new CrimeMenuOptionContext(
            attempt,
            gameSession.PreviewCrime(attempt),
            availableByType.ContainsKey(status.Attempt.Type),
            status.IsAvailable,
            status.BlockReason);
    }
}
