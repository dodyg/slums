using Slums.Core.Calendar;

namespace Slums.Core.Weather;

public sealed class WeatherProbabilityTable
{
    private static readonly Dictionary<Season, Dictionary<WeatherType, int>> Probabilities = new()
    {
        [Season.Autumn] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 70,
            [WeatherType.Hot] = 5,
            [WeatherType.CoolOvercast] = 15,
            [WeatherType.Rain] = 3,
            [WeatherType.Windy] = 5,
            [WeatherType.Heatwave] = 0,
            [WeatherType.Khamsin] = 2
        },
        [Season.Winter] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 50,
            [WeatherType.CoolOvercast] = 30,
            [WeatherType.Rain] = 5,
            [WeatherType.Windy] = 10,
            [WeatherType.Hot] = 0,
            [WeatherType.Heatwave] = 0,
            [WeatherType.Khamsin] = 5
        },
        [Season.Spring] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 40,
            [WeatherType.Hot] = 15,
            [WeatherType.Heatwave] = 3,
            [WeatherType.Khamsin] = 20,
            [WeatherType.Windy] = 15,
            [WeatherType.CoolOvercast] = 5,
            [WeatherType.Rain] = 2
        },
        [Season.Summer] = new Dictionary<WeatherType, int>
        {
            [WeatherType.Clear] = 60,
            [WeatherType.Hot] = 25,
            [WeatherType.Heatwave] = 10,
            [WeatherType.Khamsin] = 3,
            [WeatherType.CoolOvercast] = 0,
            [WeatherType.Rain] = 0,
            [WeatherType.Windy] = 2
        }
    };

    public static IReadOnlyDictionary<WeatherType, int> GetProbabilities(Season season)
    {
        return Probabilities.TryGetValue(season, out var probs) ? probs : Probabilities[Season.Autumn];
    }
}
