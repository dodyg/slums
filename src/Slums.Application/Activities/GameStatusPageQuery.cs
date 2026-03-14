using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;

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
            BuildDebtPage(context),
            BuildSkillsPage(context),
            BuildNetworkPage(context),
            BuildHeatPage(context),
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
                context.UnpaidRentDays > 0
                    ? $"Rent debt: {context.AccumulatedRentDebt} LE across {context.UnpaidRentDays} unpaid day(s)"
                    : "Rent debt: clear",
                $"Food: {household.FoodStockpile} | Medicine: {household.MedicineStock}",
                $"Local prices: food {context.FoodCost} LE | street {context.StreetFoodCost} LE | medicine {context.MedicineCost} LE",
                context.HasClinicServices
                    ? $"Clinic here: {(context.ClinicOpenToday ? "open" : "closed")} today | visit {context.ClinicVisitCost} LE | days {context.ClinicOpenDaysSummary}"
                    : "Clinic here: none",
                $"Mother: {household.MotherHealth}% {household.MotherCondition}",
                $"Mother fed today: {ToYesNo(household.FedMotherToday)}"
            ]);
    }

    private static GameStatusPage BuildDebtPage(GameStatusContext context)
    {
        var lines = new List<string>
        {
            $"Daily rent: {context.RentCost} LE",
            $"Unpaid rent days: {context.UnpaidRentDays}/{Slums.Core.Expenses.RentState.EvictionThreshold}",
            $"Accumulated rent debt: {context.AccumulatedRentDebt} LE",
            GetRentWarningStatus(context)
        };

        var owedContacts = context.Relationships.NpcRelationships.Values
            .Where(static relationship => relationship.HasUnpaidDebt)
            .Select(relationship => NpcRegistry.GetName(relationship.NpcId))
            .ToArray();

        lines.Add(owedContacts.Length == 0
            ? "Outstanding favors: none"
            : $"Outstanding favors: {string.Join(", ", owedContacts)}");

        lines.Add(context.UnpaidRentDays == 0
            ? "Rent pressure is quiet right now."
            : "Unpaid rent keeps landlord pressure active and increases the risk of eviction.");

        return new GameStatusPage("Debt", lines);
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
            lines.Add($"{NpcRegistry.GetName(npcId)}: trust {relationship.Trust}{FormatRelationshipMemory(relationship)}");
        }

        return new GameStatusPage("Network", lines);
    }

    private static GameStatusPage BuildHeatPage(GameStatusContext context)
    {
        var currentDistrictFaction = GetFactionForDistrict(context.World.CurrentDistrict);
        var currentStanding = context.Relationships.GetFactionStanding(currentDistrictFaction).Reputation;

        var lines = new List<string>
        {
            $"Police pressure: {context.PolicePressure}/100",
            GetPressureStatus(context.PolicePressure),
            $"Current district standing: {DistrictInfo.GetName(context.World.CurrentDistrict)} -> {GetFactionName(currentDistrictFaction)} {currentStanding}",
            $"Imbaba Crew {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation} | Dokki Thugs {context.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation} | Ex-Prisoner Net {context.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation}"
        };

        if (context.LastCrimeDay > 0)
        {
            lines.Add($"Last crime day: {context.LastCrimeDay}");
        }

        lines.Add(NarrativeSignalRules.HasPendingPublicWorkHeat(context.Clock.Day, context.LastCrimeDay, context.PolicePressure)
            ? "Public-facing work is currently carrying extra suspicion."
            : "Public-facing work is not carrying the extra heat signal right now.");

        lines.Add("District standing gates route access and changes who will vouch for you.");

        return new GameStatusPage("Heat", lines);
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
            var risk = definition?.RiskProfile;
            var riskSummary = risk is null
                ? "risk unknown"
                : $"fail {ToPercent(risk.WeeklyFailureChance)}% | extort {ToPercent(risk.ExtortionChance)}% | police {ToPercent(risk.PoliceHeatChance)}% | betray {ToPercent(risk.BetrayalChance)}%";
            lines.Add($"{name}: {investment.WeeklyIncomeMin}-{investment.WeeklyIncomeMax} LE/week | {state}");
            lines.Add($"  {riskSummary}");
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

    private static string GetRentWarningStatus(GameStatusContext context)
    {
        if (context.UnpaidRentDays >= Slums.Core.Expenses.RentState.EvictionThreshold)
        {
            return "Warning stage: eviction threshold reached.";
        }

        if (context.UnpaidRentDays >= Slums.Core.Expenses.RentState.FinalWarningDay)
        {
            return "Warning stage: final warning.";
        }

        if (context.UnpaidRentDays >= Slums.Core.Expenses.RentState.FirstWarningDay)
        {
            return "Warning stage: first landlord warning.";
        }

        return "Warning stage: clear.";
    }

    private static string FormatRelationshipMemory(NpcRelationship relationship)
    {
        var signals = new List<string>();

        if (relationship.HasUnpaidDebt)
        {
            signals.Add("you owe them");
        }

        if (relationship.WasHelped)
        {
            signals.Add("helped you");
        }

        if (relationship.WasEmbarrassed)
        {
            signals.Add("remembers embarrassment");
        }

        if (relationship.LastFavorDay > 0)
        {
            signals.Add($"favor on day {relationship.LastFavorDay}");
        }

        if (relationship.LastRefusalDay > 0)
        {
            signals.Add($"refused on day {relationship.LastRefusalDay}");
        }

        if (relationship.RecentContactCount > 0)
        {
            signals.Add($"recent contact {relationship.RecentContactCount}");
        }

        return signals.Count == 0 ? string.Empty : $" | {string.Join(" | ", signals.Take(2))}";
    }

    private static string GetPressureStatus(int policePressure)
    {
        return policePressure switch
        {
            >= 85 => "Heat is near arrest level and threatens the long-run endings.",
            >= 60 => "Heat is materially raising crime risk and can spill into public-facing work.",
            >= 30 => "Heat is noticeable, but not yet at a crisis point.",
            _ => "Heat is manageable for now."
        };
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

    private static string GetFactionName(FactionId factionId)
    {
        return factionId switch
        {
            FactionId.ImbabaCrew => "Imbaba Crew",
            FactionId.DokkiThugs => "Dokki Thugs",
            FactionId.ExPrisonerNetwork => "Ex-Prisoner Net",
            _ => factionId.ToString()
        };
    }

    private static int ToPercent(double chance)
    {
        return (int)Math.Round(chance * 100, MidpointRounding.AwayFromZero);
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
