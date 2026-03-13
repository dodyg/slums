using Slums.Core.Jobs;

namespace Slums.Application.Activities;

public sealed record WorkMenuOptionContext(
    JobShift Job,
    JobPreview Preview,
    JobTrackProgress Track,
    bool CanPerform,
    string? AvailabilityReason);
