using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class CrimeMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<CrimeMenuStatus> GetStatuses(GameState gameState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var location = gameState.World.GetCurrentLocation();
        if (location is null || !location.HasCrimeOpportunities)
        {
            return [];
        }

        var baseStatuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, gameState.Relationships);
        var availableByType = gameState
            .GetAvailableCrimes()
            .GroupBy(static attempt => attempt.Type)
            .ToDictionary(static group => group.Key, static group => group.Last());

        return baseStatuses
            .Select(status => BuildStatus(gameState, location, status, availableByType))
            .ToArray();
    }

    private static CrimeMenuStatus BuildStatus(
        GameState gameState,
        Location location,
        CrimeOpportunityStatus baseStatus,
        Dictionary<CrimeType, CrimeAttempt> availableByType)
    {
        var preview = gameState.PreviewCrime(baseStatus.Attempt);

        if (availableByType.TryGetValue(baseStatus.Attempt.Type, out var availableAttempt))
        {
            return new CrimeMenuStatus(
                availableAttempt,
                true,
                GetStatusText(gameState, location, availableAttempt, baseStatus.IsAvailable),
                null,
                preview.Resolution.DetectionChance,
                preview.Resolution.SuccessChance,
                preview.Resolution.PolicePressureIfDetected,
                preview.Resolution.PolicePressureIfUndetected,
                preview.ActiveModifiers,
                GetNarrativeSignals(gameState, availableAttempt));
        }

        return new CrimeMenuStatus(
            baseStatus.Attempt,
            false,
            GetStatusText(gameState, location, baseStatus.Attempt, baseStatus.IsAvailable),
            baseStatus.BlockReason,
            preview.Resolution.DetectionChance,
            preview.Resolution.SuccessChance,
            preview.Resolution.PolicePressureIfDetected,
            preview.Resolution.PolicePressureIfUndetected,
                preview.ActiveModifiers,
                GetNarrativeSignals(gameState, baseStatus.Attempt));
    }

    private static string? GetStatusText(GameState gameState, Location location, CrimeAttempt attempt, bool availableViaRegistry)
    {
        var districtStanding = gameState.Relationships.GetFactionStanding(GetFactionForDistrict(location.District)).Reputation;

        return attempt.Type switch
        {
            CrimeType.MarketFencing => $"Hanan trust: {gameState.Relationships.GetNpcRelationship(NpcId.FenceHanan).Trust}",
            CrimeType.DokkiDrop when !availableViaRegistry && IsDokkiWorkCoverUnlock(gameState, location) => "Unlocked through reliable day work in Dokki.",
            CrimeType.DokkiDrop => $"Youssef trust: {gameState.Relationships.GetNpcRelationship(NpcId.RunnerYoussef).Trust} | Dokki standing: {gameState.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            CrimeType.NetworkErrand when !availableViaRegistry && IsExPrisonerUnlock(gameState, location) => "Unlocked through ex-prisoner network standing.",
            CrimeType.NetworkErrand => $"Umm Karim trust: {gameState.Relationships.GetNpcRelationship(NpcId.FixerUmmKarim).Trust} | Imbaba standing: {gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            CrimeType.DepotFareSkim when !availableViaRegistry && IsDepotWorkUnlock(gameState, location) => "Unlocked through reliable depot work in Bulaq al-Dakrour.",
            CrimeType.DepotFareSkim => $"Safaa trust: {gameState.Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust} | Imbaba standing: {gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            CrimeType.ShubraBundleLift when !availableViaRegistry && IsLaundryWorkUnlock(gameState, location) => "Unlocked through reliable laundry work in Shubra.",
            CrimeType.ShubraBundleLift => $"Iman trust: {gameState.Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust} | Imbaba standing: {gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            _ when attempt.StreetRepRequired > 0 => $"Street rep: {districtStanding}/{attempt.StreetRepRequired}",
            _ => null
        };
    }

    private static bool IsDokkiWorkCoverUnlock(GameState gameState, Location location)
    {
        return location.Id == LocationId.Square &&
               (gameState.JobProgress.GetTrack(JobType.CallCenterWork).Reliability >= 60 ||
                gameState.JobProgress.GetTrack(JobType.CafeService).Reliability >= 60);
    }

    private static bool IsExPrisonerUnlock(GameState gameState, Location location)
    {
        return location.Id == LocationId.Market &&
               gameState.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
               gameState.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation >= 10;
    }

    private static bool IsDepotWorkUnlock(GameState gameState, Location location)
    {
        return location.Id == LocationId.Depot &&
               gameState.JobProgress.GetTrack(JobType.MicrobusDispatch).Reliability >= 60;
    }

    private static bool IsLaundryWorkUnlock(GameState gameState, Location location)
    {
        return location.Id == LocationId.Laundry &&
               gameState.JobProgress.GetTrack(JobType.LaundryPressing).Reliability >= 60;
    }

    private static FactionId GetFactionForDistrict(DistrictId districtId)
    {
        return districtId switch
        {
            DistrictId.Dokki => FactionId.DokkiThugs,
            DistrictId.ArdAlLiwa => FactionId.ExPrisonerNetwork,
            _ => FactionId.ImbabaCrew
        };
    }

    private static List<string> GetNarrativeSignals(GameState gameState, CrimeAttempt attempt)
    {
        var signals = new List<string>();

        if (!gameState.HasStoryFlag("crime_first_success"))
        {
            signals.Add("Your first successful crime still has a dedicated aftermath scene waiting.");
        }

        if (HasUnseenRouteAftermath(gameState, attempt.Type))
        {
            signals.Add("This route still has unseen first-time aftermath content.");
        }

        var projectedCrimeEarnings = gameState.TotalCrimeEarnings + Math.Max(0, attempt.BaseReward);
        var projectedCrimeCount = gameState.CrimesCommitted + 1;
        if (projectedCrimeEarnings >= 150 && projectedCrimeCount >= 2 && gameState.Player.Household.MotherHealth < 65 && !gameState.HasStoryFlag("event_mother_wrong_money_seen"))
        {
            signals.Add("Another profitable run could make home money feel suspicious tonight.");
        }

        if (gameState.PolicePressure >= 60 && gameState.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust >= 15 && !gameState.HasStoryFlag("event_neighbor_watch_seen"))
        {
            signals.Add("If tonight gets hotter, Mona is positioned to warn you before the building closes in.");
        }

        return signals;
    }

    private static bool HasUnseenRouteAftermath(GameState gameState, CrimeType crimeType)
    {
        string[] flagNames = crimeType switch
        {
            CrimeType.MarketFencing => ["crime_hanan_fence_success_seen", "crime_hanan_fence_detected_seen", "crime_hanan_fence_failure_seen"],
            CrimeType.DokkiDrop => ["crime_youssef_drop_success_seen", "crime_youssef_drop_detected_seen", "crime_youssef_drop_failure_seen"],
            CrimeType.NetworkErrand => ["crime_ummkarim_errand_success_seen", "crime_ummkarim_errand_detected_seen", "crime_ummkarim_errand_failure_seen"],
            CrimeType.DepotFareSkim => ["crime_safaa_skim_success_seen", "crime_safaa_skim_detected_seen", "crime_safaa_skim_failure_seen"],
            CrimeType.ShubraBundleLift => ["crime_iman_bundle_success_seen", "crime_iman_bundle_detected_seen", "crime_iman_bundle_failure_seen"],
            _ => []
        };

        return flagNames.Any(flagName => !gameState.HasStoryFlag(flagName));
    }
}