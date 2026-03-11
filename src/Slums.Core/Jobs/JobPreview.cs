namespace Slums.Core.Jobs;

public sealed record JobPreview(
    JobShift Job,
    string VariantReason,
    string? NextUnlockHint,
    IReadOnlyList<string> ActiveModifiers,
    string? RiskWarning);