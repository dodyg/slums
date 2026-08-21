namespace Slums.Core.World;

public static class InfrastructureImpactCalculator
{
    public static int GetTravelCostModifier(InfrastructureState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        var service = state.Get(district, InfrastructureServiceType.Transport);
        return service.Severity switch
        {
            InfrastructureSeverity.Strained => 1,
            InfrastructureSeverity.Disrupted => 3,
            _ => 0
        };
    }

    public static int GetTravelTimeModifier(InfrastructureState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        var service = state.Get(district, InfrastructureServiceType.Transport);
        return service.Severity switch
        {
            InfrastructureSeverity.Strained => 10,
            InfrastructureSeverity.Disrupted => 25,
            _ => 0
        };
    }

    public static int GetFoodStressModifier(InfrastructureState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        var service = state.Get(district, InfrastructureServiceType.Water);
        return service.Severity switch
        {
            InfrastructureSeverity.Strained => 1,
            InfrastructureSeverity.Disrupted => 3,
            _ => 0
        };
    }

    public static int GetMedicinePriceModifier(InfrastructureState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        var service = state.Get(district, InfrastructureServiceType.ClinicMedicine);
        return service.Severity switch
        {
            InfrastructureSeverity.Strained => 5,
            InfrastructureSeverity.Disrupted => 12,
            _ => 0
        };
    }

    public static int GetSleepStressModifier(InfrastructureState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        var service = state.Get(district, InfrastructureServiceType.Electricity);
        return service.Severity switch
        {
            InfrastructureSeverity.Strained => 1,
            InfrastructureSeverity.Disrupted => 3,
            _ => 0
        };
    }
}
