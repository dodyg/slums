using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.Investments;

/// <summary>Applies investment discovery, purchase, weekly resolution, and restoration.</summary>
internal static class InvestmentPurchaseService
{
    internal static IReadOnlyList<InvestmentDefinition> GetCurrentOpportunities(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var reachableNpcs = session.GetReachableNpcs().ToHashSet();
        var ownedTypes = session.InvestmentState.ActiveInvestments.Select(static investment => investment.Type).ToHashSet();
        var opportunities = new List<InvestmentDefinition>();
        foreach (var definition in InvestmentRegistry.AllDefinitions)
        {
            if (ownedTypes.Contains(definition.Type))
            {
                continue;
            }

            if (definition.OpportunityLocationId != session.World.CurrentLocationId)
            {
                continue;
            }

            if (definition.OpportunityNpc is NpcId sponsorNpc && !reachableNpcs.Contains(sponsorNpc))
            {
                continue;
            }

            opportunities.Add(definition);
        }

        return opportunities;
    }

    internal static IReadOnlyList<InvestmentDefinition> GetAvailable(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var results = new List<InvestmentDefinition>();
        foreach (var definition in GetCurrentOpportunities(session))
        {
            if (EvaluateEligibility(session, definition).IsEligible)
            {
                results.Add(definition);
            }
        }

        return results;
    }

    internal static InvestmentEligibility CheckEligibility(GameSession session, InvestmentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(definition);
        return EvaluateEligibility(session, definition);
    }

    internal static MakeInvestmentResult MakeInvestment(GameSession session, InvestmentType type)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var definition = InvestmentRegistry.GetByType(type);
        if (definition is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "MakeInvestment", before, session.CaptureStats(), $"Unknown investment type: {type}");
            return new MakeInvestmentResult(false, 0, "Unknown investment type.");
        }

        var eligibility = EvaluateEligibility(session, definition);
        if (!eligibility.IsEligible)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "MakeInvestment", before, session.CaptureStats(), string.Join(" ", eligibility.FailureReasons));
            return new MakeInvestmentResult(false, 0, string.Join(" ", eligibility.FailureReasons));
        }

        session.Player.Stats.ModifyMoney(-definition.Cost);
        session.InvestmentState.ActiveInvestments.Add(new Investment(type, definition.Cost, definition.WeeklyIncomeMin, definition.WeeklyIncomeMax, definition.RiskProfile));
        session.RaiseEvent($"Invested {definition.Cost} LE in {definition.Name}.");
        session.RecordMutation(MutationCategories.Investment, "MakeInvestment", before, session.CaptureStats(), $"Invested {definition.Cost} LE in {definition.Name}");
        return new MakeInvestmentResult(true, definition.Cost, $"Successfully invested in {definition.Name}.");
    }

    internal static InvestmentResolutionSummary ResolveWeekly(GameSession session, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var rng = random ?? session.SharedRandom;
        var summary = new InvestmentResolutionSummary();
        var schedule = session.GetCurrentSchedule();
        var toRemove = new List<Investment>();
        foreach (var investment in session.InvestmentState.ActiveInvestments)
        {
            investment.IncrementWeek();
            if (investment.IsSuspended)
            {
                var definition = InvestmentRegistry.GetByType(investment.Type);
                summary.AddResult(new InvestmentResolution(investment.Type, 0, WasLost: false, ExtortionPaid: 0, PolicePressureIncrease: 0, InvestedAmountLost: 0, $"{definition?.Name ?? investment.Type.ToString()} is recovering after last week's disruption and pays nothing this week."));
                investment.Unsuspend();
                continue;
            }

            var calculation = InvestmentResolutionCalculator.Resolve(investment, InvestmentRegistry.GetByType(investment.Type), session.Player.Stats.Money, rng);
            if (calculation.ShouldSuspend)
            {
                investment.Suspend();
                session.TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.EventInvestmentSuspensionSeen, NarrativeKnots.EventInvestmentSuspension));
            }

            var result = calculation.Resolution;
            if (result.Income > 0 && schedule.InvestmentRevenueModifier != 0)
            {
                result = result with { Income = Math.Max(0, result.Income + schedule.InvestmentRevenueModifier) };
            }

            summary.AddResult(result);
            if (result.WasLost)
            {
                toRemove.Add(investment);
            }

            if (result.Income > 0)
            {
                session.Player.Stats.ModifyMoney(result.Income);
                session.InvestmentState.TotalInvestmentEarnings += result.Income;
                if (!result.WasLost && result.ExtortionPaid == 0 && result.PolicePressureIncrease == 0)
                {
                    var investmentDef = InvestmentRegistry.GetByType(investment.Type);
                    var investmentName = investmentDef?.Name ?? investment.Type.ToString();
                    session.RaiseAutoTransaction($"{investmentName}: +{result.Income} LE weekly income.");
                }
            }

            if (result.ExtortionPaid > 0)
            {
                session.Player.Stats.ModifyMoney(-result.ExtortionPaid);
            }

            if (result.PolicePressureIncrease > 0)
            {
                session.DistrictHeat.AddHeat(session.World.CurrentDistrict, result.PolicePressureIncrease);
            }

            if (!string.IsNullOrWhiteSpace(result.Message) && (result.WasLost || result.ExtortionPaid > 0 || result.PolicePressureIncrease > 0))
            {
                session.RaiseAutoTransaction(result.Message);
            }
        }

        foreach (var investment in toRemove)
        {
            session.InvestmentState.ActiveInvestments.Remove(investment);
        }

        if (summary.TotalIncome > 0 || summary.TotalLosses > 0 || summary.TotalExtortion > 0)
        {
            session.RaiseAutoTransaction($"Weekly investments: +{summary.TotalIncome} LE income, -{summary.TotalExtortion} LE extortion, {summary.LostCount} lost.");
        }

        session.RecordMutation(MutationCategories.Investment, "ResolveWeeklyInvestments", before, session.CaptureStats(), $"Income +{summary.TotalIncome}, Extortion -{summary.TotalExtortion}, Lost {summary.LostCount}");
        return summary;
    }

    internal static void Restore(GameSession session, IEnumerable<InvestmentSnapshot> investments, int totalInvestmentEarnings)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(investments);
        session.InvestmentState.ActiveInvestments.Clear();
        foreach (var snapshot in investments)
        {
            var definition = InvestmentRegistry.GetByType(snapshot.Type);
            if (definition is not null)
            {
                session.InvestmentState.ActiveInvestments.Add(Investment.Restore(snapshot, definition.RiskProfile));
            }
        }

        session.InvestmentState.TotalInvestmentEarnings = totalInvestmentEarnings;
    }

    private static InvestmentEligibility EvaluateEligibility(GameSession session, InvestmentDefinition definition)
    {
        return InvestmentEligibilityEvaluator.Evaluate(definition, new InvestmentEligibilityContext(
            session.Player.Stats.Money,
            session.World.CurrentLocationId,
            session.GetReachableNpcs().ToHashSet(),
            session.InvestmentState.ActiveInvestments.Select(static investment => investment.Type).ToHashSet(),
            session.Relationships,
            session.TotalCrimeEarnings,
            session.Player.Skills.GetLevel(SkillId.StreetSmarts),
            session.Player.Skills.GetLevel(SkillId.Medical),
            session.Player.Skills.GetLevel(SkillId.Physical),
            session.Player.BackgroundType));
    }
}
