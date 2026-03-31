using Slums.Core.Characters;
using Slums.Core.Expenses;
using Slums.Core.Heat;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Balance;

internal sealed class BalanceRegressionTests
{
    [Test]
    public async Task DailyHungerDecay_IsTwelve()
    {
        var stats = new SurvivalStats();
        stats.ModifyHunger(-12);
        await Assert.That(stats.Hunger).IsEqualTo(80 - 12);
    }

    [Test]
    public async Task DailyStressGain_IsThree()
    {
        var stats = new SurvivalStats();
        stats.ModifyStress(3);
        await Assert.That(stats.Stress).IsEqualTo(20 + 3);
    }

    [Test]
    public async Task DailySatietyDecay_IsFifteen()
    {
        var nutrition = new NutritionState();
        nutrition.ResolveDay();
        await Assert.That(nutrition.Satiety).IsEqualTo(75 - 15);
    }

    [Test]
    public async Task MedicineCost_IsForty()
    {
        var cost = RecurringExpenses.MedicineCost;
        await Assert.That(cost).IsEqualTo(40);
    }

    [Test]
    public async Task HeatDecay_Dokki_IsSix()
    {
        var rate = HeatDecayRates.GetDecayRate(DistrictId.Dokki);
        await Assert.That(rate).IsEqualTo(6);
    }

    [Test]
    public async Task HeatDecay_Imbaba_IsFour()
    {
        var rate = HeatDecayRates.GetDecayRate(DistrictId.Imbaba);
        await Assert.That(rate).IsEqualTo(4);
    }

    [Test]
    public async Task HeatDecay_SlumDistricts_AreThree()
    {
        var bulaq = HeatDecayRates.GetDecayRate(DistrictId.BulaqAlDakrour);
        var shubra = HeatDecayRates.GetDecayRate(DistrictId.Shubra);
        var ard = HeatDecayRates.GetDecayRate(DistrictId.ArdAlLiwa);
        await Assert.That(bulaq).IsEqualTo(3);
        await Assert.That(shubra).IsEqualTo(3);
        await Assert.That(ard).IsEqualTo(3);
    }

    [Test]
    public async Task HeatDecay_Default_IsFour()
    {
        var rate = HeatDecayRates.GetDecayRate(DistrictId.DowntownCairo);
        await Assert.That(rate).IsEqualTo(4);
    }

    [Test]
    public async Task DokkiDecaysFastest_SlumsDecaySlowest()
    {
        var dokki = HeatDecayRates.GetDecayRate(DistrictId.Dokki);
        var imbaba = HeatDecayRates.GetDecayRate(DistrictId.Imbaba);
        var bulaq = HeatDecayRates.GetDecayRate(DistrictId.BulaqAlDakrour);

        await Assert.That(dokki).IsGreaterThan(imbaba);
        await Assert.That(imbaba).IsGreaterThan(bulaq);
    }
}
