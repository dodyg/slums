namespace Slums.Application.Activities;

public sealed class ClinicTravelMenuQuery
{
    public IReadOnlyList<ClinicTravelMenuStatus> GetStatuses(ClinicTravelMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Clinics
            .Select(clinic => new ClinicTravelMenuStatus(
                clinic.LocationId,
                clinic.LocationName,
                clinic.DistrictName,
                clinic.TravelCost,
                clinic.ClinicCost,
                clinic.TotalCost,
                clinic.IsOpenToday,
                clinic.OpenDaysSummary,
                clinic.TravelTimeMinutes,
                clinic.CanAfford,
                GetUnavailableReason(clinic, context)))
            .ToArray();
    }

    private static string? GetUnavailableReason(ClinicTravelOptionContext clinic, ClinicTravelMenuContext context)
    {
        if (!clinic.IsOpenToday)
        {
            return $"Closed today. Opens: {clinic.OpenDaysSummary}";
        }

        if (!clinic.CanAfford)
        {
            return $"Need {clinic.TotalCost} LE (have {context.PlayerMoney} LE)";
        }

        return null;
    }
}
