using System.Linq;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

/// <summary>
/// Validates a deserialized <see cref="GameSessionSnapshot"/> before its state is restored.
/// Broken saves (out-of-range values, unknown ids) fail as corrupt instead of restoring
/// inconsistent state.
/// </summary>
public static class SaveGameValidator
{
    public static void Validate(GameSessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var problems = new List<string>();

        if (snapshot.Clock.Day < 1)
        {
            problems.Add($"day {snapshot.Clock.Day} is below 1");
        }

        if (snapshot.Clock.Hour is < 0 or > 23)
        {
            problems.Add($"hour {snapshot.Clock.Hour} is outside 0..23");
        }

        if (snapshot.Clock.Minute is < 0 or > 59)
        {
            problems.Add($"minute {snapshot.Clock.Minute} is outside 0..59");
        }

        if (snapshot.Player.Money < 0)
        {
            problems.Add($"money {snapshot.Player.Money} is negative");
        }

        if (!IsPercentage(snapshot.Player.Energy))
        {
            problems.Add($"energy {snapshot.Player.Energy} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.Health))
        {
            problems.Add($"health {snapshot.Player.Health} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.Stress))
        {
            problems.Add($"stress {snapshot.Player.Stress} is outside 0..100");
        }

        if (snapshot.Player.Satiety is < 0 or > 100)
        {
            problems.Add($"satiety {snapshot.Player.Satiety} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.MotherHealth))
        {
            problems.Add($"mother health {snapshot.Player.MotherHealth} is outside 0..100");
        }

        if (snapshot.Player.FoodStockpile < 0)
        {
            problems.Add($"food stockpile {snapshot.Player.FoodStockpile} is negative");
        }

        if (!LocationId.All.Any(location => location.Value == snapshot.World.CurrentLocationId))
        {
            problems.Add($"current location '{snapshot.World.CurrentLocationId}' is not a declared location");
        }

        ValidateRelationships(snapshot, problems);
        ValidateJobTracks(snapshot, problems);
        ValidateCrisis(snapshot, problems);

        if (snapshot.Crime.PolicePressure is < 0 or > 100)
        {
            problems.Add($"police pressure {snapshot.Crime.PolicePressure} is outside 0..100");
        }

        if (problems.Count > 0)
        {
            throw new InvalidDataException("Save data validation failed: " + string.Join("; ", problems));
        }
    }

    private static bool IsPercentage(int value) => value is >= 0 and <= 100;

    private static void ValidateRelationships(GameSessionSnapshot snapshot, List<string> problems)
    {
        if (snapshot.Relationships is null)
        {
            problems.Add("relationships snapshot is missing");
            return;
        }

        if (snapshot.Relationships.Npcs.Count != Enum.GetValues<NpcId>().Length)
        {
            problems.Add($"relationships contain {snapshot.Relationships.Npcs.Count} NPC entries; expected {Enum.GetValues<NpcId>().Length}");
        }

        foreach (var (npcName, relationship) in snapshot.Relationships.Npcs)
        {
            if (!Enum.TryParse<NpcId>(npcName, out var npcId) || !Enum.IsDefined(npcId))
            {
                problems.Add($"relationship NPC '{npcName}' is not declared");
                continue;
            }

            if (relationship.Trust is < -100 or > 100)
            {
                problems.Add($"relationship trust for {npcId} is outside -100..100");
            }

            if (relationship.LastSeenDay < 0 || relationship.LastFavorDay < 0 || relationship.LastRefusalDay < 0 || relationship.RecentContactCount < 0)
            {
                problems.Add($"relationship memory for {npcId} contains a negative value");
            }
        }

        if (snapshot.Relationships.Factions.Count != Enum.GetValues<FactionId>().Length)
        {
            problems.Add($"relationships contain {snapshot.Relationships.Factions.Count} faction entries; expected {Enum.GetValues<FactionId>().Length}");
        }

        foreach (var (factionName, reputation) in snapshot.Relationships.Factions)
        {
            if (!Enum.TryParse<FactionId>(factionName, out var factionId) || !Enum.IsDefined(factionId))
            {
                problems.Add($"relationship faction '{factionName}' is not declared");
            }
            else if (reputation is < -100 or > 100)
            {
                problems.Add($"faction reputation for {factionId} is outside -100..100");
            }
        }
    }

    private static void ValidateJobTracks(GameSessionSnapshot snapshot, List<string> problems)
    {
        if (snapshot.JobProgress is null)
        {
            problems.Add("job progress snapshot is missing");
            return;
        }

        if (snapshot.JobProgress.Tracks.Count != Enum.GetValues<JobType>().Length)
        {
            problems.Add($"job tracks contain {snapshot.JobProgress.Tracks.Count} entries; expected {Enum.GetValues<JobType>().Length}");
        }

        foreach (var (jobName, track) in snapshot.JobProgress.Tracks)
        {
            if (!Enum.TryParse<JobType>(jobName, out var jobType) || !Enum.IsDefined(jobType))
            {
                problems.Add($"job track '{jobName}' is not declared");
                continue;
            }

            if (!IsPercentage(track.Reliability))
            {
                problems.Add($"job reliability for {jobType} is outside 0..100");
            }

            if (track.ShiftsCompleted < 0 || track.LockoutUntilDay < 0)
            {
                problems.Add($"job track for {jobType} contains a negative value");
            }
        }
    }

    private static void ValidateCrisis(GameSessionSnapshot snapshot, List<string> problems)
    {
        var crisis = snapshot.CityCrisis;
        if (crisis is null)
        {
            problems.Add("crisis snapshot is missing");
            return;
        }

        if (crisis.BeatIndex < 0 || crisis.EvidenceCollected < 0 || crisis.ResourcesCommitted < 0 || crisis.DecisionDay < 0 || crisis.CallbackDueDay < 0)
        {
            problems.Add("crisis state contains a negative progression value");
        }

        if (!IsPercentage(crisis.CooperativeCondition))
        {
            problems.Add($"crisis cooperative condition {crisis.CooperativeCondition} is outside 0..100");
        }

        if (!Enum.IsDefined(crisis.Decision) || !Enum.IsDefined(crisis.Resolution) || !Enum.IsDefined(crisis.PendingCallbackDecision))
        {
            problems.Add("crisis state contains an unknown decision or resolution");
        }
    }
}
