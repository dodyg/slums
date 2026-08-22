using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Weather;

namespace Slums.Core.Narrative;

/// <summary>
/// Immutable state needed to select a deterministic authored weather or seasonal scene.
/// </summary>
public sealed record NarrativeReachabilityContext(
    int Day,
    WeatherType Weather,
    Season Season,
    HolidayId Holiday,
    int HolidayDay,
    BackgroundType Background,
    GameDayOfWeek DayOfWeek,
    bool IsAtHome,
    bool HasCurtain,
    int MotherHealth);
