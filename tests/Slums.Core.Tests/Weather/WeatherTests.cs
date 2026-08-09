using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World;
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

    [Test]
    public async Task Khamsin_ShouldIncreaseTransportCost()
    {
        using var state = new GameSession();
        var clearCost = state.GetTravelCost(LocationId.CallCenter);

        state.RestoreWeather(WeatherType.Khamsin);

        await Assert.That(state.GetTravelCost(LocationId.CallCenter)).IsEqualTo(clearCost + 5);
        await Assert.That(state.GetTravelConditionSummary(LocationId.CallCenter)).Contains("adds 5 LE");
    }

    [Test]
    public async Task Rain_ShouldBlockPaidAndWalkingTravelToFloodProneDistricts()
    {
        using var state = new GameSession();
        state.RestoreWeather(WeatherType.Rain);
        var moneyBefore = state.Player.Stats.Money;
        var energyBefore = state.Player.Stats.Energy;

        var paidResult = state.TryTravelTo(LocationId.CallCenter);
        var walkResult = state.TryWalkTo(LocationId.Clinic);

        await Assert.That(paidResult).IsFalse();
        await Assert.That(walkResult).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(moneyBefore);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(energyBefore);
    }

    [Test]
    public async Task Rain_ShouldStillAllowTravelToNonFloodProneDistricts()
    {
        using var state = new GameSession();
        state.RestoreWeather(WeatherType.Rain);

        var result = state.TryTravelTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
    }

    [Test]
    public async Task Heatwave_ShouldRejectOutdoorWorkWithoutApplyingOutcome()
    {
        using var state = new GameSession();
        state.RestoreWeather(WeatherType.Heatwave);
        state.World.TravelTo(LocationId.FishMarket);
        var shift = new JobShift
        {
            Type = JobType.FishSorter,
            Name = "Fish Sorting",
            BasePay = 20,
            EnergyCost = 10,
            StressCost = 5,
            DurationMinutes = 60,
            MinEnergyRequired = 1
        };
        var moneyBefore = state.Player.Stats.Money;

        var result = state.WorkJob(shift, new Random(1));

        await Assert.That(result.Success).IsFalse();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(moneyBefore);
        await Assert.That(result.Message).Contains("stopped outdoor work");
    }

    [Test]
    public async Task Khamsin_ShouldBlockCrimeAtQueryAndCommandBoundaries()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Market);
        state.RestoreWeather(WeatherType.Khamsin);
        var moneyBefore = state.Player.Stats.Money;
        var crimesBefore = state.CrimesCommitted;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 30, 20, 10, 0, 5);

        var available = state.GetAvailableCrimes();
        var result = state.CommitCrime(attempt, new Random(1));

        await Assert.That(available).IsEmpty();
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("khamsin");
        await Assert.That(state.Player.Stats.Money).IsEqualTo(moneyBefore);
        await Assert.That(state.CrimesCommitted).IsEqualTo(crimesBefore);
    }

    [Test]
    public async Task WeatherCrimeDetectionModifier_ShouldReachCrimePreview()
    {
        using var state = new GameSession();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 30, 30, 10, 0, 5);
        var clearPreview = state.PreviewCrime(attempt);

        state.RestoreWeather(WeatherType.Hot);
        var hotPreview = state.PreviewCrime(attempt);

        await Assert.That(hotPreview.Resolution.DetectionChance)
            .IsEqualTo(clearPreview.Resolution.DetectionChance + 5);
        await Assert.That(hotPreview.ActiveModifiers).Contains(modifier => modifier.Contains("Hot weather", StringComparison.Ordinal));
    }
}
