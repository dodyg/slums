namespace Slums.Core.Weather;

public sealed class WeatherModifiers
{
    private static readonly WeatherState Hot = new(
        WeatherType.Hot, 5, 3, 2, 5, 0, 0, false, false, false);

    private static readonly WeatherState Heatwave = new(
        WeatherType.Heatwave, 10, 5, 5, 10, 0, -5, true, false, false);

    private static readonly WeatherState Khamsin = new(
        WeatherType.Khamsin, 8, 5, 0, 0, 5, 0, true, true, false);

    private static readonly WeatherState CoolOvercast = new(
        WeatherType.CoolOvercast, 0, -2, -2, -5, 0, 0, false, false, false);

    private static readonly WeatherState Rain = new(
        WeatherType.Rain, 0, 0, 5, 0, 0, 0, false, false, true);

    private static readonly WeatherState Windy = new(
        WeatherType.Windy, 2, 0, 0, -5, 0, 0, false, false, false);

    private static readonly Dictionary<WeatherType, WeatherState> Modifiers = new()
    {
        [WeatherType.Clear] = WeatherState.Clear,
        [WeatherType.Hot] = Hot,
        [WeatherType.Heatwave] = Heatwave,
        [WeatherType.Khamsin] = Khamsin,
        [WeatherType.CoolOvercast] = CoolOvercast,
        [WeatherType.Rain] = Rain,
        [WeatherType.Windy] = Windy
    };

    public static WeatherState GetModifiers(WeatherType type)
    {
        return Modifiers.TryGetValue(type, out var state) ? state : WeatherState.Clear;
    }

    public static string GetDisplayName(WeatherType type) => type switch
    {
        WeatherType.Clear => "Clear",
        WeatherType.Hot => "Hot",
        WeatherType.Heatwave => "Heatwave",
        WeatherType.Khamsin => "Khamsin",
        WeatherType.CoolOvercast => "Cool",
        WeatherType.Rain => "Rain",
        WeatherType.Windy => "Windy",
        _ => "Clear"
    };
}
