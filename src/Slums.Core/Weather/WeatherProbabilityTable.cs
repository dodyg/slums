using Slums.Core.Calendar;

namespace Slums.Core.Weather;

public sealed class WeatherProbabilityTable
{
    private static readonly Dictionary<Season, Dictionary<WeatherType, int>> Probabilities = new()
    {
        [Season.Autumn] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 65,
            [WeatherType.Hot] = 12,
            [WeatherType.CoolOvercast] = 12,
            [WeatherType.Rain] = 3,
            [WeatherType.Windy] = 5,
            [WeatherType.Heatwave] = 1,
            [WeatherType.Khamsin] = 2
        },
        [Season.Winter] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 52,
            [WeatherType.CoolOvercast] = 28,
            [WeatherType.Rain] = 5,
            [WeatherType.Windy] = 10,
            [WeatherType.Hot] = 3,
            [WeatherType.Heatwave] = 0,
            [WeatherType.Khamsin] = 2
        },
        [Season.Spring] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 35,
            [WeatherType.Hot] = 20,
            [WeatherType.Heatwave] = 6,
            [WeatherType.Khamsin] = 20,
            [WeatherType.Windy] = 13,
            [WeatherType.CoolOvercast] = 4,
            [WeatherType.Rain] = 2
        },
        [Season.Summer] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 48,
            [WeatherType.Hot] = 34,
            [WeatherType.Heatwave] = 15,
            [WeatherType.Khamsin] = 2,
            [WeatherType.CoolOvercast] = 0,
            [WeatherType.Rain] = 0,
            [WeatherType.Windy] = 1
        }
    };

    public static IReadOnlyDictionary<WeatherType, int> GetProbabilities(Season season)
    {
        return Probabilities.TryGetValue(season, out var probs) ? probs : Probabilities[Season.Autumn];
    }
}
