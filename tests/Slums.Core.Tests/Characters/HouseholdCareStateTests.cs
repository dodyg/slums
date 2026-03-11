using Slums.Core.Characters;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class HouseholdCareStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeHouseholdCareDefaults()
    {
        var state = new HouseholdCareState();

        await Assert.That(state.MotherHealth).IsEqualTo(70);
        await Assert.That(state.MotherCondition).IsEqualTo(MotherCondition.Stable);
        await Assert.That(state.StaplesUnits).IsEqualTo(3);
        await Assert.That(state.MedicineStock).IsEqualTo(0);
        await Assert.That(state.FedMotherToday).IsFalse();
        await Assert.That(state.MedicationGivenToday).IsFalse();
        await Assert.That(state.CheckedOnMotherToday).IsFalse();
    }

    [Test]
    public async Task SetMotherHealth_ShouldRefreshConditionToStable()
    {
        var state = new HouseholdCareState();

        state.SetMotherHealth(80);

        await Assert.That(state.MotherCondition).IsEqualTo(MotherCondition.Stable);
    }

    [Test]
    public async Task SetMotherHealth_ShouldRefreshConditionToFragile()
    {
        var state = new HouseholdCareState();

        state.SetMotherHealth(45);

        await Assert.That(state.MotherCondition).IsEqualTo(MotherCondition.Fragile);
    }

    [Test]
    public async Task SetMotherHealth_ShouldRefreshConditionToCrisis()
    {
        var state = new HouseholdCareState();

        state.SetMotherHealth(20);

        await Assert.That(state.MotherCondition).IsEqualTo(MotherCondition.Crisis);
    }

    [Test]
    public async Task SetMotherHealth_AtZero_ShouldMarkMotherDead()
    {
        var state = new HouseholdCareState();

        state.SetMotherHealth(0);

        await Assert.That(state.MotherAlive).IsFalse();
    }

    [Test]
    public async Task FeedMother_WithStaplesAvailable_ShouldConsumeStapleAndSetFlag()
    {
        var state = new HouseholdCareState();

        var result = state.FeedMother();

        await Assert.That(result).IsTrue();
        await Assert.That(state.StaplesUnits).IsEqualTo(2);
        await Assert.That(state.FedMotherToday).IsTrue();
    }

    [Test]
    public async Task FeedMother_WithoutStaples_ShouldFail()
    {
        var state = new HouseholdCareState();
        state.SetStaplesUnits(0);

        var result = state.FeedMother();

        await Assert.That(result).IsFalse();
        await Assert.That(state.FedMotherToday).IsFalse();
        await Assert.That(state.StaplesUnits).IsEqualTo(0);
    }

    [Test]
    public async Task GiveMedicine_WithStockAvailable_ShouldConsumeMedicineAndSetFlag()
    {
        var state = new HouseholdCareState();
        state.SetMedicineStock(2);

        var result = state.GiveMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.MedicineStock).IsEqualTo(1);
        await Assert.That(state.MedicationGivenToday).IsTrue();
    }

    [Test]
    public async Task GiveMedicine_WithoutStock_ShouldFail()
    {
        var state = new HouseholdCareState();

        var result = state.GiveMedicine();

        await Assert.That(result).IsFalse();
        await Assert.That(state.MedicationGivenToday).IsFalse();
    }

    [Test]
    public async Task CheckOnMother_ShouldSetCheckedFlag()
    {
        var state = new HouseholdCareState();

        state.CheckOnMother();

        await Assert.That(state.CheckedOnMotherToday).IsTrue();
    }

    [Test]
    public async Task ResolveDay_Stable_WithFoodAndMedicine_ShouldSlightlyImproveHealth()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(80);
        state.FeedMother();
        state.SetMedicineStock(1);
        state.GiveMedicine();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(2);
        await Assert.That(result.StressDelta).IsEqualTo(0);
        await Assert.That(state.MotherHealth).IsEqualTo(82);
    }

    [Test]
    public async Task ResolveDay_Stable_WithoutFood_ShouldReduceHealth()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(80);

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(-3);
        await Assert.That(state.MotherHealth).IsEqualTo(77);
    }

    [Test]
    public async Task ResolveDay_Fragile_WithFullCare_ShouldImproveHealth()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(45);
        state.FeedMother();
        state.SetMedicineStock(1);
        state.GiveMedicine();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(4);
        await Assert.That(state.MotherHealth).IsEqualTo(49);
    }

    [Test]
    public async Task ResolveDay_Fragile_WithoutMedicine_ShouldReduceHealthBy4()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(45);
        state.FeedMother();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(-4);
        await Assert.That(state.MotherHealth).IsEqualTo(41);
    }

    [Test]
    public async Task ResolveDay_Fragile_WithoutFoodOrMedicine_ShouldReduceHealthBy9()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(45);

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(-9);
        await Assert.That(state.MotherHealth).IsEqualTo(36);
    }

    [Test]
    public async Task ResolveDay_Crisis_WithFullCare_ShouldImproveHealthBy8()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(20);
        state.FeedMother();
        state.SetMedicineStock(1);
        state.GiveMedicine();
        state.CheckOnMother();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(8);
        await Assert.That(result.StressDelta).IsEqualTo(0);
        await Assert.That(state.MotherHealth).IsEqualTo(28);
    }

    [Test]
    public async Task ResolveDay_Crisis_WithoutCheck_ShouldAddPlayerStress()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(20);
        state.FeedMother();
        state.SetMedicineStock(1);
        state.GiveMedicine();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(8);
        await Assert.That(result.StressDelta).IsEqualTo(3);
    }

    [Test]
    public async Task ResolveDay_Crisis_WithoutFoodOrMedicine_ShouldReduceHealthBy15()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(20);
        state.CheckOnMother();

        var result = state.ResolveDay();

        await Assert.That(result.HealthDelta).IsEqualTo(-15);
        await Assert.That(state.MotherHealth).IsEqualTo(5);
        await Assert.That(state.MotherAlive).IsTrue();
    }

    [Test]
    public async Task ResolveDay_ShouldUpdateConditionAfterHealthChange()
    {
        var state = new HouseholdCareState();
        state.SetMotherHealth(66);

        _ = state.ResolveDay();

        await Assert.That(state.MotherHealth).IsEqualTo(63);
        await Assert.That(state.MotherCondition).IsEqualTo(MotherCondition.Fragile);
    }

    [Test]
    public async Task BeginNewDay_ShouldClearDailyCareFlags()
    {
        var state = new HouseholdCareState();
        state.FeedMother();
        state.SetMedicineStock(1);
        state.GiveMedicine();
        state.CheckOnMother();

        state.BeginNewDay();

        await Assert.That(state.FedMotherToday).IsFalse();
        await Assert.That(state.MedicationGivenToday).IsFalse();
        await Assert.That(state.CheckedOnMotherToday).IsFalse();
    }
}