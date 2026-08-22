namespace Slums.Core.Technology;

/// <summary>Persistent obligations created by Cairo's useful but uneven digital services.</summary>
public sealed class TechnologyObligationState
{
    public int HandsetDataExposure { get; private set; }
    public int MicrogridRepairDebt { get; private set; }
    public int MicrogridStorageCondition { get; private set; } = 70;
    public bool TransitPermitReview { get; private set; }
    public bool BiometricAppealPending { get; private set; }
    public int LastTelemedicineTriageDay { get; private set; }
    public int AllocationModelConfidence { get; private set; } = 58;

    public void RecordHandsetUse(int exposure = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exposure);
        HandsetDataExposure = Math.Min(100, HandsetDataExposure + exposure);
    }

    public void RecordMicrogridRepair(int partsDebt, int conditionGain = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(partsDebt);
        ArgumentOutOfRangeException.ThrowIfNegative(conditionGain);
        MicrogridRepairDebt = Math.Min(100, MicrogridRepairDebt + partsDebt);
        MicrogridStorageCondition = Math.Clamp(MicrogridStorageCondition + conditionGain, 0, 100);
    }

    public bool PayMicrogridRepairDebt(int amount)
    {
        if (amount <= 0 || MicrogridRepairDebt <= 0)
        {
            return false;
        }

        MicrogridRepairDebt = Math.Max(0, MicrogridRepairDebt - amount);
        return true;
    }

    public void RecordTransitPermitReview()
    {
        TransitPermitReview = true;
    }

    public void ResolveTransitPermitReview()
    {
        TransitPermitReview = false;
    }

    public void RecordBiometricAppeal()
    {
        BiometricAppealPending = true;
    }

    public void ResolveBiometricAppeal()
    {
        BiometricAppealPending = false;
    }

    public bool RecordTelemedicineTriage(int currentDay)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentDay);
        if (LastTelemedicineTriageDay == currentDay)
        {
            return false;
        }

        LastTelemedicineTriageDay = currentDay;
        return true;
    }

    public void SetAllocationModelConfidence(int confidence)
    {
        AllocationModelConfidence = Math.Clamp(confidence, 0, 100);
    }

    public void Restore(
        int handsetDataExposure,
        int microgridRepairDebt,
        int microgridStorageCondition,
        bool transitPermitReview,
        bool biometricAppealPending,
        int lastTelemedicineTriageDay,
        int allocationModelConfidence)
    {
        HandsetDataExposure = Math.Clamp(handsetDataExposure, 0, 100);
        MicrogridRepairDebt = Math.Clamp(microgridRepairDebt, 0, 100);
        MicrogridStorageCondition = Math.Clamp(microgridStorageCondition, 0, 100);
        TransitPermitReview = transitPermitReview;
        BiometricAppealPending = biometricAppealPending;
        LastTelemedicineTriageDay = Math.Max(0, lastTelemedicineTriageDay);
        AllocationModelConfidence = Math.Clamp(allocationModelConfidence, 0, 100);
    }
}
