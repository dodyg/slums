using Slums.Core.Investments;
using Slums.Core.Relationships;

namespace Slums.Application.Investments;

public sealed class InvestmentMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<InvestmentMenuStatus> GetStatuses(InvestmentMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Opportunities
            .Select(definition => BuildStatus(context, definition))
            .ToArray();
    }

    private static InvestmentMenuStatus BuildStatus(InvestmentMenuContext context, InvestmentDefinition definition)
    {
        var eligibility = context.EligibilityByType[definition.Type];
        context.ActiveInvestmentsByType.TryGetValue(definition.Type, out var activeInvestment);
        var opportunitySource = definition.OpportunityNpc is NpcId sponsorNpc
            ? $"Ask {NpcRegistry.GetName(sponsorNpc)} about it here."
            : "Available through local contacts here.";

        var unlockRequirements = new List<string>();

        if (definition.RequiredRelationshipNpc is NpcId requiredNpc && definition.RequiredRelationshipTrust > 0)
        {
            unlockRequirements.Add($"Need trust {definition.RequiredRelationshipTrust} with {NpcRegistry.GetName(requiredNpc)}.");
        }

        if (definition.RequiresStreetSmartsOrExPrisoner)
        {
            unlockRequirements.Add("Need Street Smarts 2+ or the released-prisoner background.");
        }

        if (definition.RequiresCrimePath)
        {
            unlockRequirements.Add("Needs an active crime path.");
        }

        if (unlockRequirements.Count == 0)
        {
            unlockRequirements.Add("No special unlock beyond the cash.");
        }

        return new InvestmentMenuStatus(
            definition,
            eligibility.IsEligible,
            eligibility.FailureReasons,
            opportunitySource,
            $"{definition.WeeklyIncomeMin}-{definition.WeeklyIncomeMax} LE / week",
            string.Join(" ", unlockRequirements),
            BuildRiskBreakdown(definition),
            GetOwnedStateSummary(activeInvestment),
            GetCurrentStateNotes(definition, activeInvestment));
    }

    private static List<string> BuildRiskBreakdown(InvestmentDefinition definition)
    {
        var profile = definition.RiskProfile;
        var breakdown = new List<string>
        {
            $"Failure {ToPercent(profile.WeeklyFailureChance)}% | Extortion {ToPercent(profile.ExtortionChance)}% | Police {ToPercent(profile.PoliceHeatChance)}% | Betrayal {ToPercent(profile.BetrayalChance)}%"
        };

        if (profile.ExtortionChance > 0)
        {
            breakdown.Add($"Typical extortion hit: {profile.ExtortionAmountMin}-{profile.ExtortionAmountMax} LE.");
        }

        return breakdown;
    }

    private static string? GetOwnedStateSummary(Investment? activeInvestment)
    {
        if (activeInvestment is null)
        {
            return null;
        }

        return activeInvestment.IsSuspended
            ? $"Already active. Week {activeInvestment.WeeksActive}. Suspended this week after a disruption."
            : $"Already active. Week {activeInvestment.WeeksActive} and currently paying normally.";
    }

    private static List<string> GetCurrentStateNotes(InvestmentDefinition definition, Investment? activeInvestment)
    {
        var notes = new List<string>();

        if (definition.RiskProfile.ExtortionChance > 0)
        {
            notes.Add("If extortion hits and you cannot cover it, the venture suspends for a recovery week.");
        }

        if (definition.RiskProfile.PoliceHeatChance > 0)
        {
            notes.Add("Police trouble does not always kill the venture, but it pushes heat upward.");
        }

        if (activeInvestment?.IsSuspended == true)
        {
            notes.Add("A suspended venture pays nothing this week, then automatically attempts to recover next week.");
        }

        return notes;
    }

    private static int ToPercent(double chance)
    {
        return (int)Math.Round(chance * 100, MidpointRounding.AwayFromZero);
    }
}
