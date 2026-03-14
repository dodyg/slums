using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Narrative;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.World;
using Slums.Application.Narrative;

namespace Slums.Application.Activities;

public sealed class CrimeMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<CrimeMenuStatus> GetStatuses(CrimeMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Location is null || !context.Location.HasCrimeOpportunities)
        {
            return [];
        }

        return context.Options.Select(option => BuildStatus(context, option)).ToArray();
    }

    private static CrimeMenuStatus BuildStatus(CrimeMenuContext context, CrimeMenuOptionContext option)
    {
        return new CrimeMenuStatus(
            option.Attempt,
            option.IsAvailable,
            GetStatusText(context, option.Attempt, option.AvailableViaRegistry),
            option.BlockReason,
            option.Preview.Resolution.DetectionChance,
            option.Preview.Resolution.SuccessChance,
            option.Preview.Resolution.PolicePressureIfDetected,
            option.Preview.Resolution.PolicePressureIfUndetected,
            option.Preview.ActiveModifiers,
            GetNarrativeSignals(context, option.Attempt));
    }

    private static string? GetStatusText(CrimeMenuContext context, CrimeAttempt attempt, bool availableViaRegistry)
    {
        var location = context.Location ?? throw new InvalidOperationException("Crime menu context requires a location.");
        var districtStanding = context.Relationships.GetFactionStanding(GetFactionForDistrict(location.District)).Reputation;

        return attempt.Type switch
        {
            CrimeType.MarketFencing => $"Hanan trust: {context.Relationships.GetNpcRelationship(NpcId.FenceHanan).Trust}",
            CrimeType.DokkiDrop when !availableViaRegistry && IsDokkiWorkCoverUnlock(context, location) => "Unlocked through reliable day work in Dokki.",
            CrimeType.DokkiDrop => $"Youssef trust: {context.Relationships.GetNpcRelationship(NpcId.RunnerYoussef).Trust} | Dokki standing: {context.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            CrimeType.NetworkErrand when !availableViaRegistry && IsExPrisonerUnlock(context, location) => "Unlocked through ex-prisoner network standing.",
            CrimeType.NetworkErrand => $"Umm Karim trust: {context.Relationships.GetNpcRelationship(NpcId.FixerUmmKarim).Trust} | Imbaba standing: {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            CrimeType.DepotFareSkim when !availableViaRegistry && IsDepotWorkUnlock(context, location) => "Unlocked through reliable depot work in Bulaq al-Dakrour.",
            CrimeType.DepotFareSkim => $"Safaa trust: {context.Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust} | Imbaba standing: {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            CrimeType.ShubraBundleLift when !availableViaRegistry && IsLaundryWorkUnlock(context, location) => "Unlocked through reliable laundry work in Shubra.",
            CrimeType.ShubraBundleLift => $"Iman trust: {context.Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust} | Imbaba standing: {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            _ when attempt.StreetRepRequired > 0 => $"Street rep: {districtStanding}/{attempt.StreetRepRequired}",
            _ => null
        };
    }

    private static bool IsDokkiWorkCoverUnlock(CrimeMenuContext context, Location location)
    {
        return location.Id == LocationId.Square &&
               (context.JobProgress.GetTrack(JobType.CallCenterWork).Reliability >= 60 ||
                context.JobProgress.GetTrack(JobType.CafeService).Reliability >= 60);
    }

    private static bool IsExPrisonerUnlock(CrimeMenuContext context, Location location)
    {
        return location.Id == LocationId.Market &&
               context.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
               context.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation >= 10;
    }

    private static bool IsDepotWorkUnlock(CrimeMenuContext context, Location location)
    {
        return location.Id == LocationId.Depot &&
               context.JobProgress.GetTrack(JobType.MicrobusDispatch).Reliability >= 60;
    }

    private static bool IsLaundryWorkUnlock(CrimeMenuContext context, Location location)
    {
        return location.Id == LocationId.Laundry &&
               context.JobProgress.GetTrack(JobType.LaundryPressing).Reliability >= 60;
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

    private static List<string> GetNarrativeSignals(CrimeMenuContext context, CrimeAttempt attempt)
    {
        var signals = new List<string>();

        if (NarrativeSignalRules.HasPendingFirstCrimeAftermath(context.StoryFlags))
        {
            signals.Add("Your first successful crime still has a dedicated aftermath scene waiting.");
        }

        if (HasUnseenRouteAftermath(context, attempt.Type))
        {
            signals.Add("This route still has unseen first-time aftermath content.");
        }

        var projectedCrimeEarnings = context.TotalCrimeEarnings + Math.Max(0, attempt.BaseReward);
        var projectedCrimeCount = context.CrimesCommitted + 1;
        if (NarrativeSignalRules.HasPendingMotherWrongMoney(context.Player, projectedCrimeEarnings, projectedCrimeCount, context.StoryFlags))
        {
            signals.Add("Another profitable run could make home money feel suspicious tonight.");
        }

        if (NarrativeSignalRules.HasPendingNeighborWatch(context.PolicePressure, context.Relationships, context.StoryFlags))
        {
            signals.Add("If tonight gets hotter, Mona is positioned to warn you before the building closes in.");
        }

        return signals;
    }

    private static bool HasUnseenRouteAftermath(CrimeMenuContext context, CrimeType crimeType)
    {
        return StoryFlags.GetCrimeRouteAftermathFlags(crimeType)
            .Any(flagName => !context.HasStoryFlag(flagName));
    }
}
