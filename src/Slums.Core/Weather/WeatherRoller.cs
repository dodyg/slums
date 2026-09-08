using Slums.Core.Calendar;

namespace Slums.Core.Weather;

public static class WeatherRoller
{
    public static WeatherType Roll(Season season, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var probabilities = WeatherProbabilityTable.GetProbabilities(season);
        var roll = random.Next(100);
        var cumulative = 0;

        foreach (var (type, weight) in probabilities)
        {
            if (weight <= 0)
            {
                continue;
            }

            cumulative += weight;
            if (roll < cumulative)
            {
                return type;
            }
        }

        return WeatherType.Clear;
    }
}
