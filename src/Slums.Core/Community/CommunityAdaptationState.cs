namespace Slums.Core.Community;

/// <summary>Persisted group-level results from locally coordinated adaptation work.</summary>
public sealed class CommunityAdaptationState
{
    public int CoolingRoomDaysRemaining { get; private set; }
    public int WaterReserveUnits { get; private set; }
    public int SuccessfulActions { get; private set; }
    public int ShelterContributions { get; private set; }

    public void AddCoolingRoomDays(int days)
    {
        CoolingRoomDaysRemaining = Math.Min(14, CoolingRoomDaysRemaining + Math.Max(0, days));
    }

    public void AddWaterReserve(int units)
    {
        WaterReserveUnits = Math.Min(10, WaterReserveUnits + Math.Max(0, units));
    }

    public bool TryConsumeWaterReserve()
    {
        if (WaterReserveUnits <= 0)
        {
            return false;
        }

        WaterReserveUnits--;
        return true;
    }

    public void RecordSuccessfulAction(int shelterContribution)
    {
        SuccessfulActions++;
        ShelterContributions = Math.Clamp(ShelterContributions + Math.Max(0, shelterContribution), 0, 100);
    }

    public void AdvanceDay()
    {
        CoolingRoomDaysRemaining = Math.Max(0, CoolingRoomDaysRemaining - 1);
    }

    public void Restore(int coolingRoomDaysRemaining, int waterReserveUnits, int successfulActions, int shelterContributions)
    {
        CoolingRoomDaysRemaining = Math.Clamp(coolingRoomDaysRemaining, 0, 14);
        WaterReserveUnits = Math.Clamp(waterReserveUnits, 0, 10);
        SuccessfulActions = Math.Max(0, successfulActions);
        ShelterContributions = Math.Clamp(shelterContributions, 0, 100);
    }
}
