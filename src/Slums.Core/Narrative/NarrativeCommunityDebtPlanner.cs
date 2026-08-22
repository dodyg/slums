using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.Territory;

namespace Slums.Core.Narrative;

/// <summary>
/// Selects one-time authored community and debt scenes from observable simulation state.
/// </summary>
public static class NarrativeCommunityDebtPlanner
{
    private static readonly IReadOnlyList<NarrativeSceneTrigger> Empty = [];

    /// <summary>Returns all currently eligible scenes in stable priority order.</summary>
    public static IReadOnlyList<NarrativeSceneTrigger> GetTriggers(
        NarrativeCommunityDebtContext context,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(storyFlags);

        var triggers = new List<NarrativeSceneTrigger>(capacity: 4);
        AddCommunityTriggers(context, storyFlags, triggers);
        AddDebtTriggers(context, storyFlags, triggers);
        return triggers.Count == 0 ? Empty : triggers;
    }

    private static void AddCommunityTriggers(
        NarrativeCommunityDebtContext context,
        IReadOnlySet<string> storyFlags,
        List<NarrativeSceneTrigger> triggers)
    {
        if (context.DayOfWeek == Slums.Core.Clock.GameDayOfWeek.Friday
            && context.Background is not BackgroundType.ReleasedPoliticalPrisoner
            && context.Background is not BackgroundType.MedicalSchoolDropout
            && context.CommunityAttendance == 0)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityFridayRooftopSeen, "event_friday_rooftop");
        }

        if (context.Day >= 3 && context.Day % 14 == 3)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityNeighborhoodCleanupSeen, "event_neighborhood_cleanup");
        }

        if (context.HasTeaCircleInvitation && context.CommunityAttendance >= 1)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityRooftopTeaSeen, "event_rooftop_tea");
        }

        if (context.Day >= 12 && context.Day % 30 == 12 && context.CommunityAttendance >= 1)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityMulidSeen, "event_mulid");
        }

        if (context.ImbabaTensionLevel >= TensionLevel.High)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityStreetArgumentSeen, "event_street_argument");
        }

        if (context.CrimesCommitted > 0 && context.PolicePressure >= 35)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityProtectionDemandSeen, "event_protection_demand");
        }

        if (context.ImbabaTension >= 45 && context.CommunityAttendance >= 1)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityAllianceShiftSeen, "event_alliance_shift");
        }

        if (context.PolicePressure >= 50)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityPoliceCrackdownSeen, "event_police_crackdown");
        }

        if (context.ImbabaControlledByDokkiThugs || context.ImbabaControlledByExPrisonerNetwork)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityTerritoryFlipSeen, "event_territory_flip");
        }

        if (context.Background == BackgroundType.SudaneseRefugee && context.ImbabaTensionLevel >= TensionLevel.Elevated)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityRefugeeSolidaritySeen, "event_refugee_solidarity");
        }

        if (context.ConsecutiveCommunitySkips >= 3)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityIsolationSignalSeen, "event_isolation_signal");
        }

        if (context.Background == BackgroundType.ReleasedPoliticalPrisoner
            && context.DayOfWeek == Slums.Core.Clock.GameDayOfWeek.Friday)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityFridayPrisonerSeen, "event_friday_prisoner");
        }

        if (context.Background == BackgroundType.MedicalSchoolDropout
            && context.DayOfWeek == Slums.Core.Clock.GameDayOfWeek.Friday)
        {
            Add(triggers, storyFlags, StoryFlags.CommunityFridayMedicalSeen, "event_friday_medical");
        }
    }

    private static void AddDebtTriggers(
        NarrativeCommunityDebtContext context,
        IReadOnlySet<string> storyFlags,
        List<NarrativeSceneTrigger> triggers)
    {
        if (context.HasLoanSharkDebt && context.LoanSharkDaysOverdue >= 1)
        {
            Add(triggers, storyFlags, StoryFlags.DebtLoanSharkFirstWarningSeen, "event_loan_shark_first_warning");
        }

        if (context.HasLoanSharkDebt && context.LoanSharkDaysOverdue >= 3)
        {
            Add(triggers, storyFlags, StoryFlags.DebtLoanSharkVisitSeen, "event_loan_shark_visit");
        }

        if (context.HasLoanSharkDebt && context.LoanSharkDaysOverdue >= 7)
        {
            Add(triggers, storyFlags, StoryFlags.DebtLoanSharkUltimatumSeen, "event_loan_shark_ultimatum");
        }

        if (context.Day >= 7 && context.MonaTrust >= 10 && !context.HasNeighborDebt)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcLoanRequestMonaSeen, "event_npc_loan_request_mona");
        }

        if (context.Day >= 7 && context.YoussefTrust >= 10 && context.CrimesCommitted > 0)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcLoanRequestYoussefSeen, "event_npc_loan_request_youssef");
        }

        if (context.MonaWasHelped && context.Day >= 4)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcLoanRepayMonaSeen, "event_npc_loan_repay_mona");
        }

        if (context.YoussefWasHelped && context.Day >= 4)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcLoanRepayYoussefSeen, "event_npc_loan_repay_youssef");
        }

        if (context.MonaTrust >= 10 && context.Day >= 9)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcHardshipMonaSeen, "event_npc_hardship_mona");
        }

        if (context.NadiaTrust >= 10 && context.Day >= 10)
        {
            Add(triggers, storyFlags, StoryFlags.DebtNpcWindfallNadiaSeen, "event_npc_windfall_nadia");
        }

        if (context.HasLoanSharkDebt || context.HasNeighborDebt)
        {
            Add(triggers, storyFlags, StoryFlags.DebtGossipSeen, "event_debt_gossip");
        }

        if (context.CrimesCommitted >= 2 || context.PolicePressure >= 40)
        {
            Add(triggers, storyFlags, StoryFlags.DebtRumorWarningSeen, "event_rumor_warning");
        }

        if (context.CommunityAttendance >= 2 && context.Day >= 8)
        {
            Add(triggers, storyFlags, StoryFlags.DebtCommunityCircleSeen, "event_community_debt_circle");
        }
    }

    private static void Add(
        List<NarrativeSceneTrigger> triggers,
        IReadOnlySet<string> storyFlags,
        string flagName,
        string knotName)
    {
        if (!storyFlags.Contains(flagName))
        {
            triggers.Add(new NarrativeSceneTrigger(flagName, knotName));
        }
    }
}
