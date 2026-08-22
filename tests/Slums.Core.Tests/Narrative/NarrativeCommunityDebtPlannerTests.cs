using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Narrative;
using Slums.Core.Territory;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeCommunityDebtPlannerTests
{
    [Test]
    public void Planner_CoversEveryDisconnectedCommunityAndDebtKnot()
    {
        var contexts = new Dictionary<string, NarrativeCommunityDebtContext>
        {
            ["event_friday_rooftop"] = Context(dayOfWeek: GameDayOfWeek.Friday, background: BackgroundType.SudaneseRefugee),
            ["event_neighborhood_cleanup"] = Context(day: 3),
            ["event_rooftop_tea"] = Context(attendance: 1, hasTeaInvitation: true),
            ["event_mulid"] = Context(day: 12, attendance: 1),
            ["event_street_argument"] = Context(tension: 80, tensionLevel: TensionLevel.Dangerous),
            ["event_protection_demand"] = Context(crimes: 1, policePressure: 40),
            ["event_alliance_shift"] = Context(tension: 50, attendance: 1),
            ["event_police_crackdown"] = Context(policePressure: 50),
            ["event_territory_flip"] = Context(dokkiControlsImbaba: true),
            ["event_refugee_solidarity"] = Context(background: BackgroundType.SudaneseRefugee, tensionLevel: TensionLevel.Elevated),
            ["event_isolation_signal"] = Context(skips: 3),
            ["event_friday_prisoner"] = Context(background: BackgroundType.ReleasedPoliticalPrisoner, dayOfWeek: GameDayOfWeek.Friday),
            ["event_friday_medical"] = Context(background: BackgroundType.MedicalSchoolDropout, dayOfWeek: GameDayOfWeek.Friday),
            ["event_loan_shark_first_warning"] = Context(hasLoanSharkDebt: true, loanSharkDaysOverdue: 1),
            ["event_loan_shark_visit"] = Context(hasLoanSharkDebt: true, loanSharkDaysOverdue: 3),
            ["event_loan_shark_ultimatum"] = Context(hasLoanSharkDebt: true, loanSharkDaysOverdue: 7),
            ["event_npc_loan_request_mona"] = Context(day: 7, monaTrust: 10),
            ["event_npc_loan_request_youssef"] = Context(day: 7, youssefTrust: 10, crimes: 1),
            ["event_npc_loan_repay_mona"] = Context(day: 4, monaWasHelped: true),
            ["event_npc_loan_repay_youssef"] = Context(day: 4, youssefWasHelped: true),
            ["event_npc_hardship_mona"] = Context(day: 9, monaTrust: 10),
            ["event_npc_windfall_nadia"] = Context(day: 10, nadiaTrust: 10),
            ["event_debt_gossip"] = Context(hasLoanSharkDebt: true),
            ["event_rumor_warning"] = Context(crimes: 2),
            ["event_community_debt_circle"] = Context(day: 8, attendance: 2)
        };

        foreach (var (expectedKnot, context) in contexts)
        {
            var knots = NarrativeCommunityDebtPlanner.GetTriggers(context, new HashSet<string>())
                .Select(static trigger => trigger.KnotName);

            knots.Should().Contain(expectedKnot);
        }
    }

    [Test]
    public void Planner_DoesNotReplayTriggeredScenes()
    {
        var flags = new HashSet<string> { StoryFlags.DebtGossipSeen };
        var triggers = NarrativeCommunityDebtPlanner.GetTriggers(Context(hasLoanSharkDebt: true), flags);

        triggers.Should().NotContain(trigger => trigger.KnotName == "event_debt_gossip");
    }

    private static NarrativeCommunityDebtContext Context(
        int day = 1,
        GameDayOfWeek dayOfWeek = GameDayOfWeek.Saturday,
        BackgroundType background = BackgroundType.MedicalSchoolDropout,
        int attendance = 0,
        int skips = 0,
        bool hasTeaInvitation = false,
        int policePressure = 0,
        int crimes = 0,
        int monaTrust = 0,
        int youssefTrust = 0,
        int nadiaTrust = 0,
        bool monaWasHelped = false,
        bool youssefWasHelped = false,
        bool hasLoanSharkDebt = false,
        int loanSharkDaysOverdue = 0,
        bool hasNeighborDebt = false,
        int tension = 20,
        TensionLevel tensionLevel = TensionLevel.Normal,
        bool dokkiControlsImbaba = false)
    {
        return new NarrativeCommunityDebtContext(
            day,
            dayOfWeek,
            background,
            attendance,
            skips,
            hasTeaInvitation,
            policePressure,
            crimes,
            0,
            monaTrust,
            youssefTrust,
            nadiaTrust,
            0,
            monaWasHelped,
            youssefWasHelped,
            hasLoanSharkDebt,
            loanSharkDaysOverdue,
            0,
            hasNeighborDebt,
            tension,
            tensionLevel,
            dokkiControlsImbaba,
            false);
    }
}
