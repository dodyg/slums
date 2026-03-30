using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.Weather;
using TUnit.Core;

namespace Slums.Core.Tests.Weather;

internal sealed class WeatherTests
{
    [Test]
    public async Task WeatherModifiers_Clear_IsBaseline()
    {
        var clear = WeatherModifiers.GetModifiers(WeatherType.Clear);

        await Assert.That(clear.EnergyDrainModifier).IsEqualTo(0);
        await Assert.That(clear.StressModifier).IsEqualTo(0);
        await Assert.That(clear.FoodCostModifier).IsEqualTo(0);
        await Assert.That(clear.CrimeDetectionModifier).IsEqualTo(0);
        await Assert.That(clear.BlocksOutdoorJobs).IsFalse();
        await Assert.That(clear.BlocksCrime).IsFalse();
    }

    [Test]
    public async Task WeatherModifiers_Hot_MatchesRequirements()
    {
        var hot = WeatherModifiers.GetModifiers(WeatherType.Hot);

        await Assert.That(hot.EnergyDrainModifier).IsEqualTo(5);
        await Assert.That(hot.StressModifier).IsEqualTo(3);
        await Assert.That(hot.FoodCostModifier).IsEqualTo(2);
        await Assert.That(hot.CrimeDetectionModifier).IsEqualTo(5);
        await Assert.That(hot.BlocksOutdoorJobs).IsFalse();
    }

    [Test]
    public async Task WeatherModifiers_Heatwave_MatchesRequirements()
    {
        var heatwave = WeatherModifiers.GetModifiers(WeatherType.Heatwave);

        await Assert.That(heatwave.EnergyDrainModifier).IsEqualTo(10);
        await Assert.That(heatwave.StressModifier).IsEqualTo(5);
        await Assert.That(heatwave.FoodCostModifier).IsEqualTo(5);
        await Assert.That(heatwave.CrimeDetectionModifier).IsEqualTo(10);
        await Assert.That(heatwave.BlocksOutdoorJobs).IsTrue();
        await Assert.That(heatwave.HealthModifier).IsEqualTo(-5);
    }

    [Test]
    public async Task WeatherModifiers_Khamsin_BlocksOutdoorJobsAndCrime()
    {
        var khamsin = WeatherModifiers.GetModifiers(WeatherType.Khamsin);

        await Assert.That(khamsin.EnergyDrainModifier).IsEqualTo(8);
        await Assert.That(khamsin.StressModifier).IsEqualTo(5);
        await Assert.That(khamsin.BlocksOutdoorJobs).IsTrue();
        await Assert.That(khamsin.BlocksCrime).IsTrue();
        await Assert.That(khamsin.TravelCostModifier).IsEqualTo(5);
    }

    [Test]
    public async Task WeatherModifiers_CoolOvercast_HasBonuses()
    {
        var cool = WeatherModifiers.GetModifiers(WeatherType.CoolOvercast);

        await Assert.That(cool.FoodCostModifier).IsEqualTo(-2);
        await Assert.That(cool.CrimeDetectionModifier).IsEqualTo(-5);
        await Assert.That(cool.StressModifier).IsEqualTo(-2);
    }

    [Test]
    public async Task WeatherModifiers_Rain_BlocksTravelToFloodProneAreas()
    {
        var rain = WeatherModifiers.GetModifiers(WeatherType.Rain);

        await Assert.That(rain.FoodCostModifier).IsEqualTo(5);
        await Assert.That(rain.BlocksTravelToFloodProneAreas).IsTrue();
    }

    [Test]
    public async Task WeatherModifiers_Windy_HasModifiers()
    {
        var windy = WeatherModifiers.GetModifiers(WeatherType.Windy);

        await Assert.That(windy.EnergyDrainModifier).IsEqualTo(2);
        await Assert.That(windy.CrimeDetectionModifier).IsEqualTo(-5);
    }

    [Test]
    public async Task WeatherProbabilityTable_AllSeasonsHaveWeights()
    {
        foreach (Season season in Enum.GetValues<Season>())
        {
            var probs = WeatherProbabilityTable.GetProbabilities(season);
            var total = probs.Values.Sum();
            await Assert.That(total).IsEqualTo(100);
        }
    }

    [Test]
    public async Task WeatherRoller_ProducesValidTypes()
    {
        var rng = new Random(42);
        for (var i = 0; i < 100; i++)
        {
            foreach (Season season in Enum.GetValues<Season>())
            {
                var result = WeatherRoller.Roll(season, rng);
                await Assert.That(Enum.IsDefined(result)).IsTrue();
            }
        }
    }

    [Test]
    public async Task WeatherRoller_AutumnMostlyClear()
    {
        var clearCount = 0;
        var rng = new Random(42);
        for (var i = 0; i < 100; i++)
        {
            var result = WeatherRoller.Roll(Season.Autumn, rng);
            if (result == WeatherType.Clear)
            {
                clearCount++;
            }
        }

        await Assert.That(clearCount).IsGreaterThan(50);
    }

    [Test]
    public async Task GameSession_DefaultWeather_IsClear()
    {
        using var state = new GameSession();

        await Assert.That(state.CurrentWeather.Type).IsEqualTo(WeatherType.Clear);
    }

    [Test]
    public async Task GameSession_EndDay_RollsNewWeather()
    {
        var rng = new Random(42);
        using var state = new GameSession(rng);
        state.Player.Nutrition.Eat(MealQuality.Basic);

        var weatherBefore = state.CurrentWeather.Type;
        state.EndDay(rng);
        var weatherAfter = state.CurrentWeather.Type;

        await Assert.That(Enum.IsDefined(weatherAfter)).IsTrue();
    }

    [Test]
    public async Task GameSession_RestoreWeather_PreservesState()
    {
        using var state = new GameSession();
        state.RestoreWeather(WeatherType.Khamsin);

        await Assert.That(state.CurrentWeather.Type).IsEqualTo(WeatherType.Khamsin);
        await Assert.That(state.CurrentWeather.BlocksOutdoorJobs).IsTrue();
        await Assert.That(state.CurrentWeather.BlocksCrime).IsTrue();
    }

    [Test]
    public async Task WeatherModifiers_GetDisplayName_ReturnsCorrectNames()
    {
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Clear)).IsEqualTo("Clear");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Hot)).IsEqualTo("Hot");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Heatwave)).IsEqualTo("Heatwave");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Khamsin)).IsEqualTo("Khamsin");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.CoolOvercast)).IsEqualTo("Cool");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Rain)).IsEqualTo("Rain");
        await Assert.That(WeatherModifiers.GetDisplayName(WeatherType.Windy)).IsEqualTo("Windy");
    }
}
