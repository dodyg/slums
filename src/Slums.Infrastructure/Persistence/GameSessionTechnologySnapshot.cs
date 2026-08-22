using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionTechnologySnapshot
{
    public int HandsetDataExposure { get; init; }
    public int MicrogridRepairDebt { get; init; }
    public int MicrogridStorageCondition { get; init; } = 70;
    public bool TransitPermitReview { get; init; }
    public bool BiometricAppealPending { get; init; }
    public int LastTelemedicineTriageDay { get; init; }
    public int AllocationModelConfidence { get; init; } = 58;

    public static GameSessionTechnologySnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionTechnologySnapshot
        {
            HandsetDataExposure = gameSession.Technology.HandsetDataExposure,
            MicrogridRepairDebt = gameSession.Technology.MicrogridRepairDebt,
            MicrogridStorageCondition = gameSession.Technology.MicrogridStorageCondition,
            TransitPermitReview = gameSession.Technology.TransitPermitReview,
            BiometricAppealPending = gameSession.Technology.BiometricAppealPending,
            LastTelemedicineTriageDay = gameSession.Technology.LastTelemedicineTriageDay,
            AllocationModelConfidence = gameSession.Technology.AllocationModelConfidence
        };
    }
}
