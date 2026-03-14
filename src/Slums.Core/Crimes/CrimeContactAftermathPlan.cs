using Slums.Core.Narrative;

namespace Slums.Core.Crimes;

public sealed record CrimeContactAftermathPlan(
    int PolicePressureReduction,
    string HeatMessage,
    NarrativeSceneTrigger HeatTrigger,
    int FailureMoneyGain,
    int FailureStressRelief,
    string? FailureMessage,
    NarrativeSceneTrigger? FailureTrigger);
