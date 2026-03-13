using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record ClinicTravelMenuContext(
    IReadOnlyList<ClinicTravelOptionContext> Clinics,
    int PlayerMoney,
    int MotherHealth)
{
    public static ClinicTravelMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var clinicLocations = gameSession.GetClinicLocations();
        var options = clinicLocations
            .Select(location =>
            {
                var option = gameSession.GetClinicTravelOption(location.Id);
                return new ClinicTravelOptionContext(
                    option.LocationId,
                    option.LocationName,
                    option.DistrictName,
                    option.TravelCost,
                    option.ClinicCost,
                    option.TotalCost,
                    option.IsOpenToday,
                    option.OpenDaysSummary,
                    option.TravelTimeMinutes,
                    option.CanAfford,
                    option.IsValidOption);
            })
            .Where(option => option.IsValidOption)
            .ToArray();

        return new ClinicTravelMenuContext(
            options,
            gameSession.Player.Stats.Money,
            gameSession.Player.Household.MotherHealth);
    }
}
