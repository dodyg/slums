using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;
using Slums.Application.Narrative;

namespace Slums.Application.Activities;

public sealed class GameStatusPageQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<GameStatusPage> GetPages(GameStatusContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return
        [
            BuildSurvivalPage(context),
            BuildSkillsPage(context),
            BuildNetworkPage(context),
            BuildInvestmentsPage(context),
            BuildSignalsPage(context),
            BuildProgressPage(context)
        ];
    }

    private static GameStatusPage BuildSurvivalPage(GameStatusContext context)
    {
        var household = context.Player.Household;
        var location = context.World.GetCurrentLocation()?.Name ?? "Unknown";

        return new GameStatusPage(
            "Survival",
            [
                $"Day {context.Clock.Day} at {context.Clock.Hour:D2}:{context.Clock.Minute:D2}",
                $"Location: {location}",
                $"District: {DistrictInfo.GetName(context.World.CurrentDistrict)}",
                $"Money: {context.Player.Stats.Money} LE",
                $"Police pressure: {context.PolicePressure}",
                $"Food: {household.FoodStockpile} | Medicine: {household.MedicineStock}",
                $"Local prices: food {context.FoodCost} LE | street {context.StreetFoodCost} LE | medicine {context.MedicineCost} LE",
                context.HasClinicServices
                    ? $"Clinic here: {(context.ClinicOpenToday ? "open" : "closed")} today | visit {context.ClinicVisitCost} LE | days {context.ClinicOpenDaysSummary}"
                    : "Clinic here: none",
                $"Mother: {household.MotherHealth}% {household.MotherCondition}",
                $"Mother fed today: {ToYesNo(household.FedMotherToday)}"
            ]);
    }

    private static GameStatusPage BuildSkillsPage(GameStatusContext context)
    {
        var lines = Enum
            .GetValues<SkillId>()
            .Select(skillId => $"{GetSkillName(skillId)}: {context.Player.Skills.GetLevel(skillId)}")
            .ToList();

        lines.Add($"Medicine price: {context.MedicineCost} LE");

        return new GameStatusPage("Skills", lines);
    }

    private static GameStatusPage BuildNetworkPage(GameStatusContext context)
    {
        var lines = new List<string>
        {
            $"Imbaba Crew: {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            $"Dokki Thugs: {context.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            $"Ex-Prisoner Net: {context.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation}"
        };

        foreach (var npcId in GetKeyNpcOrder())
        {
            var relationship = context.Relationships.GetNpcRelationship(npcId);
            lines.Add($"{NpcRegistry.GetName(npcId)}: trust {relationship.Trust}");
        }

        return new GameStatusPage("Network", lines);
    }

    private static GameStatusPage BuildProgressPage(GameStatusContext context)
    {
        var lines = new List<string>
        {
            $"Days survived: {context.DaysSurvived}",
            $"Honest shifts: {context.HonestShiftsCompleted}",
            $"Honest earnings: {context.TotalHonestWorkEarnings} LE",
            $"Crimes committed: {context.CrimesCommitted}",
            $"Crime earnings: {context.TotalCrimeEarnings} LE",
            $"Last crime day: {context.LastCrimeDay}",
            $"Last honest-work day: {context.LastHonestWorkDay}"
        };

        lines.AddRange(GetTrajectoryHints(context).Select(static hint => $"Hint: {hint}"));
        return new GameStatusPage("Progress", lines);
    }

    private static GameStatusPage BuildInvestmentsPage(GameStatusContext context)
    {
        var lines = new List<string>
        {
            $"Active investments: {context.ActiveInvestments.Count}",
            $"Total investment earnings: {context.TotalInvestmentEarnings} LE"
        };

        if (context.ActiveInvestments.Count == 0)
        {
            lines.Add("No active investments yet.");
            lines.Add("Talk to trusted contacts in the right places to find opportunities.");
            return new GameStatusPage("Investments", lines);
        }

        foreach (var investment in context.ActiveInvestments)
        {
            var definition = InvestmentRegistry.GetByType(investment.Type);
            var name = definition?.Name ?? investment.Type.ToString();
            var state = investment.IsSuspended ? "Suspended this week" : $"Week {investment.WeeksActive}";
            lines.Add($"{name}: {investment.WeeklyIncomeMin}-{investment.WeeklyIncomeMax} LE/week | {state}");
        }

        return new GameStatusPage("Investments", lines);
    }

    private static GameStatusPage BuildSignalsPage(GameStatusContext context)
    {
        var lines = new List<string>();

        if (NarrativeSignalRules.HasPendingPublicWorkHeat(context.Clock.Day, context.LastCrimeDay, context.PolicePressure))
        {
            lines.Add("Public-facing work is likely to trigger suspicion while street heat is this high.");
        }

        if (NarrativeSignalRules.HasPendingMotherWrongMoney(context.Player, context.TotalCrimeEarnings, context.CrimesCommitted, context.StoryFlags))
        {
            lines.Add("Home is primed for a tense reaction to sudden money.");
        }

        if (NarrativeSignalRules.HasPendingNeighborWatch(context.PolicePressure, context.Relationships, context.StoryFlags))
        {
            lines.Add("Mona is currently positioned to warn you if the building gets watched.");
        }

        if (NarrativeSignalRules.HasPendingMedicalClinicReflection(context.Player, context.StoryFlags))
        {
            lines.Add("A successful clinic shift can still trigger the medical-dropout clinic reflection.");
        }

        if (NarrativeSignalRules.HasPendingSalmaMedicineHelp(context.Player, context.Relationships))
        {
            lines.Add("Salma is in a position to quietly help with medicine after a good clinic day.");
        }

        if (context.CrimesCommitted == 0 && NarrativeSignalRules.HasPendingFirstCrimeAftermath(context.StoryFlags))
        {
            lines.Add("Your first successful crime still has a unique aftermath beat waiting.");
        }

        if (lines.Count == 0)
        {
            lines.Add("No high-priority narrative signals are active right now.");
        }

        return new GameStatusPage("Signals", lines);
    }

    private static IEnumerable<string> GetTrajectoryHints(GameStatusContext context)
    {
        if (context.CrimesCommitted == 0)
        {
            yield return "Clean run intact so far.";
        }

        if (context.PolicePressure >= 85)
        {
            yield return "Heat is high enough to threaten arrest or a buried-by-heat ending.";
        }

        if (context.Player.Stats.Money > 500 && context.CrimesCommitted <= 3 && context.Player.Household.MotherHealth > 60)
        {
            yield return "Luxor route conditions are currently strong.";
        }

        if (context.TotalCrimeEarnings >= 1000 && context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation > 50)
        {
            yield return "Crime-kingpin route is within reach.";
        }

        if (context.CrimesCommitted > 0 && context.LastCrimeDay > 0 && context.Clock.Day - context.LastCrimeDay >= 5)
        {
            yield return "You have been away from crime long enough to keep a leaving-crime route alive.";
        }

        if (context.DaysSurvived >= 30 && context.CrimesCommitted == 0 && context.Player.Stats.Money > 200 && context.PolicePressure < 20)
        {
            yield return "Honest-stability conditions are already met.";
        }
    }

    private static IEnumerable<NpcId> GetKeyNpcOrder()
    {
        yield return NpcId.LandlordHajjMahmoud;
        yield return NpcId.NeighborMona;
        yield return NpcId.NurseSalma;
        yield return NpcId.PharmacistMariam;
        yield return NpcId.DispatcherSafaa;
        yield return NpcId.LaundryOwnerIman;
        yield return NpcId.FixerUmmKarim;
        yield return NpcId.FenceHanan;
        yield return NpcId.RunnerYoussef;
    }

    private static string GetSkillName(SkillId skillId)
    {
        return skillId switch
        {
            SkillId.StreetSmarts => "Street Smarts",
            _ => skillId.ToString()
        };
    }

    private static string ToYesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}
