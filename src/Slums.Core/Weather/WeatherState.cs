namespace Slums.Core.Weather;

public sealed record WeatherState(
    WeatherType Type,
    int EnergyDrainModifier,
    int StressModifier,
    int FoodCostModifier,
    int CrimeDetectionModifier,
    int TravelCostModifier,
    int HealthModifier,
    bool BlocksOutdoorJobs,
    bool BlocksCrime,
    bool BlocksTravelToFloodProneAreas)
{
    public static WeatherState Clear { get; } = new(WeatherType.Clear, 0, 0, 0, 0, 0, 0, false, false, false);
}
