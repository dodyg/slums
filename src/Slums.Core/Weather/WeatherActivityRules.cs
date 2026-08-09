using Slums.Core.Jobs;
using Slums.Core.World;

namespace Slums.Core.Weather;

public static class WeatherActivityRules
{
    public static bool BlocksTravelTo(WeatherState weather, DistrictId destinationDistrict)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return weather.BlocksTravelToFloodProneAreas &&
            destinationDistrict is DistrictId.ArdAlLiwa or DistrictId.Dokki;
    }

    public static bool BlocksJob(WeatherState weather, JobType jobType)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return weather.BlocksOutdoorJobs && jobType is
            JobType.StreetVending or
            JobType.FishSorter or
            JobType.MarketPorter or
            JobType.MicrobusDispatch;
    }

    public static string GetTravelBlockReason(WeatherState weather, DistrictId destinationDistrict)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return weather.Type == WeatherType.Rain && BlocksTravelTo(weather, destinationDistrict)
            ? "Flooded streets make that district unreachable today."
            : "Weather makes that route unreachable today.";
    }

    public static string GetJobBlockReason(WeatherState weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return $"{WeatherModifiers.GetDisplayName(weather.Type)} conditions have stopped outdoor work.";
    }

    public static string GetCrimeBlockReason(WeatherState weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return weather.Type == WeatherType.Khamsin
            ? "The khamsin has shut down street activity. Crime routes are closed today."
            : "The weather has closed crime routes today.";
    }
}
