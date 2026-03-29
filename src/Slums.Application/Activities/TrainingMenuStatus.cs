using Slums.Core.Training;

namespace Slums.Application.Activities;

public sealed record TrainingMenuStatus(
    TrainingActivity Activity,
    bool CanAfford,
    bool HasEnergy,
    bool RightTime,
    bool NpcTrustMet,
    bool NotTrainedToday,
    bool NotAtCap,
    bool CanTrain,
    string? UnavailabilityReason);
