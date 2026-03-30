using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Calendar;

internal sealed class SeasonalCalendarTests
{
    [Test]
    public async Task GameCalendar_GetDate_Day1_ReturnsOctober1()
    {
        var date = GameCalendar.GetDate(1);

        await Assert.That(date).IsEqualTo(new DateOnly(2024, 10, 1));
    }

    [Test]
    public async Task GameCalendar_GetDate_Day2_ReturnsOctober2()
    {
        var date = GameCalendar.GetDate(2);

        await Assert.That(date).IsEqualTo(new DateOnly(2024, 10, 2));
    }

    [Test]
    public async Task GameCalendar_GetSeason_Day1_ReturnsAutumn()
    {
        var season = GameCalendar.GetSeason(1);

        await Assert.That(season).IsEqualTo(Season.Autumn);
    }

    [Test]
    public async Task GameCalendar_GetSeason_October31_ReturnsAutumn()
    {
        var season = GameCalendar.GetSeason(31);

        await Assert.That(season).IsEqualTo(Season.Autumn);
    }

    [Test]
    public async Task GameCalendar_GetSeason_November30_ReturnsAutumn()
    {
        var season = GameCalendar.GetSeason(61);

        await Assert.That(season).IsEqualTo(Season.Autumn);
    }

    [Test]
    public async Task GameCalendar_GetSeason_December1_ReturnsWinter()
    {
        var season = GameCalendar.GetSeason(62);

        await Assert.That(season).IsEqualTo(Season.Winter);
    }

    [Test]
    public async Task GameCalendar_GetSeason_January1_ReturnsWinter()
    {
        var season = GameCalendar.GetSeason(93);

        await Assert.That(season).IsEqualTo(Season.Winter);
    }

    [Test]
    public async Task GameCalendar_GetSeason_February28_ReturnsWinter()
    {
        var season = GameCalendar.GetSeason(151);

        await Assert.That(season).IsEqualTo(Season.Winter);
    }

    [Test]
    public async Task GameCalendar_GetSeason_March1_ReturnsSpring()
    {
        var season = GameCalendar.GetSeason(152);

        await Assert.That(season).IsEqualTo(Season.Spring);
    }

    [Test]
    public async Task GameCalendar_GetSeason_May31_ReturnsSpring()
    {
        var season = GameCalendar.GetSeason(243);

        await Assert.That(season).IsEqualTo(Season.Spring);
    }

    [Test]
    public async Task GameCalendar_GetSeason_June1_ReturnsSummer()
    {
        var season = GameCalendar.GetSeason(244);

        await Assert.That(season).IsEqualTo(Season.Summer);
    }

    [Test]
    public async Task GameCalendar_GetSeason_September30_ReturnsSummer()
    {
        var season = GameCalendar.GetSeason(365);

        await Assert.That(season).IsEqualTo(Season.Summer);
    }

    [Test]
    public async Task GameCalendar_GetSeasonName_ReturnsCorrectNames()
    {
        await Assert.That(GameCalendar.GetSeasonName(Season.Autumn)).IsEqualTo("Autumn");
        await Assert.That(GameCalendar.GetSeasonName(Season.Winter)).IsEqualTo("Winter");
        await Assert.That(GameCalendar.GetSeasonName(Season.Spring)).IsEqualTo("Spring");
        await Assert.That(GameCalendar.GetSeasonName(Season.Summer)).IsEqualTo("Summer");
    }

    [Test]
    public async Task GameCalendar_GetMonth_Day1_Returns10()
    {
        var month = GameCalendar.GetMonth(1);

        await Assert.That(month).IsEqualTo(10);
    }

    [Test]
    public async Task GameCalendar_GetDayOfMonth_Day1_Returns1()
    {
        var day = GameCalendar.GetDayOfMonth(1);

        await Assert.That(day).IsEqualTo(1);
    }

    [Test]
    public async Task SeasonModifiersRegistry_HasAllFourSeasons()
    {
        await Assert.That(SeasonModifiersRegistry.AllModifiers.Count).IsEqualTo(4);
        await Assert.That(SeasonModifiersRegistry.AllModifiers.ContainsKey(Season.Autumn)).IsTrue();
        await Assert.That(SeasonModifiersRegistry.AllModifiers.ContainsKey(Season.Winter)).IsTrue();
        await Assert.That(SeasonModifiersRegistry.AllModifiers.ContainsKey(Season.Spring)).IsTrue();
        await Assert.That(SeasonModifiersRegistry.AllModifiers.ContainsKey(Season.Summer)).IsTrue();
    }

    [Test]
    public async Task SeasonModifiersRegistry_Autumn_IsBaseline()
    {
        var autumn = SeasonModifiersRegistry.GetModifiers(Season.Autumn);

        await Assert.That(autumn.FoodCostModifier).IsEqualTo(0);
        await Assert.That(autumn.EnergyDrainModifier).IsEqualTo(0);
        await Assert.That(autumn.StressModifier).IsEqualTo(0);
        await Assert.That(autumn.RestRecoveryBonus).IsEqualTo(0);
        await Assert.That(autumn.InvestmentReturnModifierPercent).IsEqualTo(0);
        await Assert.That(autumn.IllnessEventFrequencyMultiplier).IsEqualTo(1.0);
        await Assert.That(autumn.OutdoorWorkStressModifier).IsEqualTo(0);
    }

    [Test]
    public async Task SeasonModifiersRegistry_Winter_MatchesRequirements()
    {
        var winter = SeasonModifiersRegistry.GetModifiers(Season.Winter);

        await Assert.That(winter.FoodCostModifier).IsEqualTo(-2);
        await Assert.That(winter.EnergyDrainModifier).IsEqualTo(0);
        await Assert.That(winter.StressModifier).IsEqualTo(0);
        await Assert.That(winter.RestRecoveryBonus).IsEqualTo(3);
        await Assert.That(winter.InvestmentReturnModifierPercent).IsEqualTo(0);
        await Assert.That(winter.IllnessEventFrequencyMultiplier).IsEqualTo(1.5);
        await Assert.That(winter.OutdoorWorkStressModifier).IsEqualTo(0);
    }

    [Test]
    public async Task SeasonModifiersRegistry_Spring_MatchesRequirements()
    {
        var spring = SeasonModifiersRegistry.GetModifiers(Season.Spring);

        await Assert.That(spring.FoodCostModifier).IsEqualTo(2);
        await Assert.That(spring.EnergyDrainModifier).IsEqualTo(0);
        await Assert.That(spring.StressModifier).IsEqualTo(0);
        await Assert.That(spring.RestRecoveryBonus).IsEqualTo(0);
        await Assert.That(spring.InvestmentReturnModifierPercent).IsEqualTo(10);
        await Assert.That(spring.IllnessEventFrequencyMultiplier).IsEqualTo(1.0);
        await Assert.That(spring.OutdoorWorkStressModifier).IsEqualTo(0);
    }

    [Test]
    public async Task SeasonModifiersRegistry_Summer_MatchesRequirements()
    {
        var summer = SeasonModifiersRegistry.GetModifiers(Season.Summer);

        await Assert.That(summer.FoodCostModifier).IsEqualTo(0);
        await Assert.That(summer.EnergyDrainModifier).IsEqualTo(5);
        await Assert.That(summer.StressModifier).IsEqualTo(3);
        await Assert.That(summer.RestRecoveryBonus).IsEqualTo(0);
        await Assert.That(summer.InvestmentReturnModifierPercent).IsEqualTo(0);
        await Assert.That(summer.IllnessEventFrequencyMultiplier).IsEqualTo(1.0);
        await Assert.That(summer.OutdoorWorkStressModifier).IsEqualTo(3);
    }

    [Test]
    public async Task SeasonModifiers_None_IsBaselineAutumn()
    {
        var none = SeasonModifiers.None;

        await Assert.That(none.Season).IsEqualTo(Season.Autumn);
        await Assert.That(none.FoodCostModifier).IsEqualTo(0);
        await Assert.That(none.EnergyDrainModifier).IsEqualTo(0);
        await Assert.That(none.StressModifier).IsEqualTo(0);
    }

    [Test]
    public async Task GameSession_GetCurrentSeason_Day1_ReturnsAutumn()
    {
        using var state = new GameSession();

        var season = state.GetCurrentSeason();

        await Assert.That(season).IsEqualTo(Season.Autumn);
    }

    [Test]
    public async Task GameSession_GetCurrentSeasonModifiers_Day1_ReturnsAutumnModifiers()
    {
        using var state = new GameSession();

        var modifiers = state.GetCurrentSeasonModifiers();

        await Assert.That(modifiers.Season).IsEqualTo(Season.Autumn);
        await Assert.That(modifiers.FoodCostModifier).IsEqualTo(0);
    }

    [Test]
    public async Task GameSession_EndDay_SummerAppliesMoreEnergyDrainThanAutumn()
    {
        using var autumn = new GameSession();
        autumn.Player.Stats.SetEnergy(70);
        autumn.Player.Stats.SetStress(10);
        autumn.Player.Nutrition.Eat(MealQuality.Basic);

        using var summer = new GameSession();
        summer.Clock.SetTime(244, 6, 0);
        summer.Player.Stats.SetEnergy(70);
        summer.Player.Stats.SetStress(10);
        summer.Player.Nutrition.Eat(MealQuality.Basic);

        autumn.EndDay();
        summer.EndDay();

        await Assert.That(summer.Player.Stats.Energy).IsLessThan(autumn.Player.Stats.Energy);
    }

    [Test]
    public async Task GameSession_EndDay_SummerAppliesMoreStressThanAutumn()
    {
        using var autumn = new GameSession();
        autumn.Player.Stats.SetEnergy(70);
        autumn.Player.Stats.SetStress(10);
        autumn.Player.Nutrition.Eat(MealQuality.Basic);

        using var summer = new GameSession();
        summer.Clock.SetTime(244, 6, 0);
        summer.Player.Stats.SetEnergy(70);
        summer.Player.Stats.SetStress(10);
        summer.Player.Nutrition.Eat(MealQuality.Basic);

        autumn.EndDay();
        summer.EndDay();

        await Assert.That(summer.Player.Stats.Stress).IsGreaterThan(autumn.Player.Stats.Stress);
    }

    [Test]
    public async Task GameSession_EndDay_WinterAppliesExtraRestRecovery()
    {
        using var autumn = new GameSession();
        autumn.Player.Stats.SetEnergy(30);
        autumn.Player.Nutrition.Eat(MealQuality.Basic);

        using var winter = new GameSession();
        winter.Clock.SetTime(62, 6, 0);
        winter.Player.Stats.SetEnergy(30);
        winter.Player.Nutrition.Eat(MealQuality.Basic);

        autumn.EndDay();
        winter.EndDay();

        await Assert.That(winter.Player.Stats.Energy).IsGreaterThan(autumn.Player.Stats.Energy);
    }

    [Test]
    public async Task GameSession_GetFoodCost_WinterAppliesNegativeModifier()
    {
        using var state = new GameSession();
        state.Clock.SetTime(64, 6, 0);

        int autumnCost;
        using (var autumn = new GameSession())
        {
            autumnCost = autumn.GetFoodCost();
        }

        state.Player.Stats.SetMoney(100);
        var winterCost = state.GetFoodCost();

        await Assert.That(winterCost).IsLessThan(autumnCost);
    }

    [Test]
    public async Task GameSession_GetFoodCost_SpringAppliesPositiveModifier()
    {
        using var state = new GameSession();
        state.Clock.SetTime(152, 6, 0);

        int autumnCost;
        using (var autumn = new GameSession())
        {
            autumnCost = autumn.GetFoodCost();
        }

        state.Player.Stats.SetMoney(100);
        var springCost = state.GetFoodCost();

        await Assert.That(springCost).IsGreaterThan(autumnCost);
    }
}
