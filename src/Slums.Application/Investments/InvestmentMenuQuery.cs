using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Investments;

public sealed class InvestmentMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<InvestmentMenuStatus> GetStatuses(InvestmentMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Opportunities
            .Select(definition => BuildStatus(context.GameSession, definition))
            .ToArray();
    }

    private static InvestmentMenuStatus BuildStatus(GameSession gameSession, InvestmentDefinition definition)
    {
        var eligibility = gameSession.CheckInvestmentEligibility(definition);
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
            string.Join(" ", unlockRequirements));
    }
}
