using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Investments;

public sealed record InvestmentDefinition
{
    public InvestmentType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string RiskLabel { get; init; } = string.Empty;
    public int Cost { get; init; }
    public int WeeklyIncomeMin { get; init; }
    public int WeeklyIncomeMax { get; init; }
    public InvestmentRiskProfile RiskProfile { get; init; } = InvestmentRiskProfile.Low;
    public NpcId? RequiredRelationshipNpc { get; init; }
    public int RequiredRelationshipTrust { get; init; }
    public bool RequiresCrimePath { get; init; }
    public bool RequiresStreetSmartsOrExPrisoner { get; init; }
    public int? RequiredMedicalLevel { get; init; }
    public int? RequiredPhysicalLevel { get; init; }
    public NpcId? OpportunityNpc { get; init; }
    public LocationId OpportunityLocationId { get; init; }
}
