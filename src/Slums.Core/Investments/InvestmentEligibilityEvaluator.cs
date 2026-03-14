using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Investments;

public static class InvestmentEligibilityEvaluator
{
    public static InvestmentEligibility Evaluate(InvestmentDefinition definition, InvestmentEligibilityContext context)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(context);

        var reasons = new List<string>();

        if (context.CurrentMoney < definition.Cost)
        {
            reasons.Add($"Not enough money. Cost: {definition.Cost} LE.");
        }

        if (definition.OpportunityLocationId != context.CurrentLocationId)
        {
            reasons.Add($"This opportunity is only discussed at {GetLocationName(definition.OpportunityLocationId)}.");
        }

        if (definition.OpportunityNpc is NpcId sponsorNpc && !context.ReachableNpcs.Contains(sponsorNpc))
        {
            reasons.Add($"{NpcRegistry.GetName(sponsorNpc)} is not available to discuss this here right now.");
        }

        if (context.OwnedInvestmentTypes.Contains(definition.Type))
        {
            reasons.Add("You already have this investment.");
        }

        if (definition.RequiredRelationshipNpc is NpcId npcId &&
            definition.RequiredRelationshipTrust > 0)
        {
            var trust = context.Relationships.GetNpcRelationship(npcId).Trust;
            if (trust < definition.RequiredRelationshipTrust)
            {
                reasons.Add($"Need {definition.RequiredRelationshipTrust} trust with {NpcRegistry.GetName(npcId)}. Current: {trust}.");
            }
        }

        if (definition.RequiresCrimePath && context.TotalCrimeEarnings < 50)
        {
            reasons.Add("Requires active involvement in crime operations.");
        }

        if (definition.RequiresStreetSmartsOrExPrisoner &&
            context.StreetSmartsLevel < 2 &&
            context.BackgroundType != BackgroundType.ReleasedPoliticalPrisoner)
        {
            reasons.Add("Requires street smarts (level 2+) or ex-prisoner background.");
        }

        return new InvestmentEligibility(reasons.Count == 0, reasons);
    }

    private static string GetLocationName(LocationId locationId)
    {
        return WorldState.AllLocations.FirstOrDefault(location => location.Id == locationId)?.Name ?? locationId.Value;
    }
}
