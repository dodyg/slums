using Slums.Core.Jobs;

namespace Slums.Application.Activities;

public sealed record WorkMenuStatus(
    JobShift Job,
    int Reliability,
    int ShiftsCompleted,
    int? LockoutUntilDay,
    bool CanPerform,
    string? AvailabilityReason,
    string VariantReason,
    string? NextUnlockHint,
    IReadOnlyList<string> ActiveModifiers,
    string? RiskWarning,
    IReadOnlyList<string> NarrativeSignals);