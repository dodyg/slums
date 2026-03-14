using Slums.Core.Narrative;

namespace Slums.Core.Jobs;

public sealed record WorkSpilloverPlan(
    int StressDelta,
    int EmployerTrustDelta,
    string Message,
    NarrativeSceneTrigger? NarrativeTrigger);
