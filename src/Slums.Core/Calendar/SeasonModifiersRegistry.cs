namespace Slums.Core.Calendar;

public sealed record SeasonModifiersRegistry
{
    private static readonly SeasonModifiers Autumn = new(Season.Autumn, "Autumn", 0, 0, 0, 0, 0, 1.0, 0);
    private static readonly SeasonModifiers Winter = new(Season.Winter, "Winter", -2, 0, 0, 3, 0, 1.5, 0);
    private static readonly SeasonModifiers Spring = new(Season.Spring, "Spring", 2, 0, 0, 0, 10, 1.0, 0);
    private static readonly SeasonModifiers Summer = new(Season.Summer, "Summer", 0, 5, 3, 0, 0, 1.0, 3);

    public static IReadOnlyDictionary<Season, SeasonModifiers> AllModifiers { get; } =
        new Dictionary<Season, SeasonModifiers>
        {
            [Season.Autumn] = Autumn,
            [Season.Winter] = Winter,
            [Season.Spring] = Spring,
            [Season.Summer] = Summer
        };

    public static SeasonModifiers GetModifiers(Season season)
    {
        return AllModifiers.TryGetValue(season, out var modifiers)
            ? modifiers
            : Autumn;
    }
}
