using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Heat;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Heat;

internal sealed class DistrictHeatTests
{
    [Test]
    public async Task DistrictHeatEntry_WithHeat_ClampsToZero()
    {
        var entry = new DistrictHeatEntry(DistrictId.Imbaba, 5, 3, 0);

        var result = entry.WithHeat(-10);

        await Assert.That(result.Heat).IsEqualTo(0);
    }

    [Test]
    public async Task DistrictHeatEntry_WithHeat_ClampsToHundred()
    {
        var entry = new DistrictHeatEntry(DistrictId.Imbaba, 95, 3, 0);

        var result = entry.WithHeat(150);

        await Assert.That(result.Heat).IsEqualTo(100);
    }

    [Test]
    public async Task DistrictHeatState_GetHeat_DefaultIsZero()
    {
        var state = new DistrictHeatState();

        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(0);
        await Assert.That(state.GetHeat(DistrictId.Dokki)).IsEqualTo(0);
    }

    [Test]
    public async Task DistrictHeatState_AddHeat_IncreasesHeat()
    {
        var state = new DistrictHeatState();

        state.AddHeat(DistrictId.Imbaba, 20);

        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(20);
    }

    [Test]
    public async Task DistrictHeatState_AddHeat_ClampsToHundred()
    {
        var state = new DistrictHeatState();

        state.AddHeat(DistrictId.Imbaba, 150);

        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(100);
    }

    [Test]
    public async Task DistrictHeatState_AddHeat_ClampsToZero()
    {
        var state = new DistrictHeatState();

        state.AddHeat(DistrictId.Imbaba, -10);

        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(0);
    }

    [Test]
    public async Task DistrictHeatState_SetHeat_SetsExactValue()
    {
        var state = new DistrictHeatState();

        state.SetHeat(DistrictId.Dokki, 42);

        await Assert.That(state.GetHeat(DistrictId.Dokki)).IsEqualTo(42);
    }

    [Test]
    public async Task DistrictHeatState_SetHeatAll_SetsAllDistricts()
    {
        var state = new DistrictHeatState();

        state.SetHeatAll(50);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            await Assert.That(state.GetHeat(district)).IsEqualTo(50);
        }
    }

    [Test]
    public async Task DistrictHeatState_DecayAll_DokkiDecaysFastest()
    {
        var state = new DistrictHeatState();
        state.SetHeatAll(50);

        state.DecayAll();

        await Assert.That(state.GetHeat(DistrictId.Dokki)).IsEqualTo(45);
    }

    [Test]
    public async Task DistrictHeatState_DecayAll_ImbabaModerateDecay()
    {
        var state = new DistrictHeatState();
        state.SetHeatAll(50);

        state.DecayAll();

        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(47);
    }

    [Test]
    public async Task DistrictHeatState_DecayAll_SlumsDecaySlowest()
    {
        var state = new DistrictHeatState();
        state.SetHeatAll(50);

        state.DecayAll();

        await Assert.That(state.GetHeat(DistrictId.BulaqAlDakrour)).IsEqualTo(48);
        await Assert.That(state.GetHeat(DistrictId.Shubra)).IsEqualTo(48);
        await Assert.That(state.GetHeat(DistrictId.ArdAlLiwa)).IsEqualTo(48);
    }

    [Test]
    public async Task DistrictHeatState_DecayAll_DoesNotGoBelowBaseline()
    {
        var state = new DistrictHeatState();
        state.SetBaselineHeat(DistrictId.Dokki, 10);
        state.SetHeat(DistrictId.Dokki, 12);

        state.DecayAll();

        await Assert.That(state.GetHeat(DistrictId.Dokki)).IsEqualTo(10);
    }

    [Test]
    public async Task DistrictHeatState_DecayAll_WithModifierAppliesRate()
    {
        var state = new DistrictHeatState();
        state.SetHeatAll(50);
        state.DecayRateModifier = 0.5;

        state.DecayAll();

        await Assert.That(state.GetHeat(DistrictId.Dokki)).IsEqualTo(48);
        await Assert.That(state.GetHeat(DistrictId.Imbaba)).IsEqualTo(49);
    }

    [Test]
    public async Task DistrictHeatState_GetGlobalPressure_ReturnsMaxHeat()
    {
        var state = new DistrictHeatState();
        state.AddHeat(DistrictId.Imbaba, 30);
        state.AddHeat(DistrictId.Dokki, 50);
        state.AddHeat(DistrictId.BulaqAlDakrour, 20);

        await Assert.That(state.GetGlobalPressure()).IsEqualTo(50);
    }

    [Test]
    public async Task DistrictHeatState_GetGlobalPressure_AllZero_ReturnsZero()
    {
        var state = new DistrictHeatState();

        await Assert.That(state.GetGlobalPressure()).IsEqualTo(0);
    }

    [Test]
    public async Task DistrictHeatState_GetHighHeatDistricts_ReturnsAboveThreshold()
    {
        var state = new DistrictHeatState();
        state.AddHeat(DistrictId.Imbaba, 70);
        state.AddHeat(DistrictId.Dokki, 85);
        state.AddHeat(DistrictId.BulaqAlDakrour, 40);

        var highHeat = state.GetHighHeatDistricts(60);

        await Assert.That(highHeat).Contains(DistrictId.Imbaba);
        await Assert.That(highHeat).Contains(DistrictId.Dokki);
        await Assert.That(highHeat.Count).IsEqualTo(2);
    }

    [Test]
    public async Task DistrictHeatState_ApplyBleedOver_TransfersFromHighToLow()
    {
        var state = new DistrictHeatState();
        state.AddHeat(DistrictId.Imbaba, 100);
        state.AddHeat(DistrictId.BulaqAlDakrour, 0);

        state.ApplyBleedOver();

        var imbabaHeat = state.GetHeat(DistrictId.Imbaba);
        var bulaqHeat = state.GetHeat(DistrictId.BulaqAlDakrour);
        await Assert.That(bulaqHeat).IsGreaterThan(0);
        await Assert.That(imbabaHeat).IsLessThan(100);
    }

    [Test]
    public async Task DistrictHeatState_ApplyBleedOver_ImbabaBulaqRateIsHigherThanImbabaDokki()
    {
        var state1 = new DistrictHeatState();
        state1.AddHeat(DistrictId.Imbaba, 100);
        state1.AddHeat(DistrictId.BulaqAlDakrour, 0);
        state1.ApplyBleedOver();
        var bulaqTransfer = state1.GetHeat(DistrictId.BulaqAlDakrour);

        var state2 = new DistrictHeatState();
        state2.AddHeat(DistrictId.Imbaba, 100);
        state2.AddHeat(DistrictId.Dokki, 0);
        state2.ApplyBleedOver();
        var dokkiTransfer = state2.GetHeat(DistrictId.Dokki);

        await Assert.That(bulaqTransfer).IsGreaterThan(dokkiTransfer);
    }

    [Test]
    public async Task DistrictHeatState_ApplyBleedOver_OnlyTransfersFromHighToLow()
    {
        var state = new DistrictHeatState();
        state.AddHeat(DistrictId.Dokki, 50);

        state.ApplyBleedOver();

        var dokkiHeat = state.GetHeat(DistrictId.Dokki);
        var imbabaHeat = state.GetHeat(DistrictId.Imbaba);
        await Assert.That(dokkiHeat).IsLessThan(50);
        await Assert.That(imbabaHeat).IsGreaterThan(0);
    }

    [Test]
    public async Task DistrictHeatState_SetBaselineHeat_SetsFloor()
    {
        var state = new DistrictHeatState();

        state.SetBaselineHeat(DistrictId.Dokki, 10);

        var entry = state.Entries[DistrictId.Dokki];
        await Assert.That(entry.BaselineHeat).IsEqualTo(10);
        await Assert.That(entry.Heat).IsEqualTo(10);
    }

    [Test]
    public async Task DistrictHeatState_RestoreEntry_OverwritesEntry()
    {
        var state = new DistrictHeatState();

        state.RestoreEntry(DistrictId.Imbaba, 55, 2, 5);

        var entry = state.Entries[DistrictId.Imbaba];
        await Assert.That(entry.Heat).IsEqualTo(55);
        await Assert.That(entry.DecayRate).IsEqualTo(2);
        await Assert.That(entry.BaselineHeat).IsEqualTo(5);
    }

    [Test]
    public async Task GameSession_PolicePressure_IsComputedFromDistrictHeat()
    {
        using var state = new GameSession();

        state.DistrictHeat.AddHeat(DistrictId.Imbaba, 40);
        state.DistrictHeat.AddHeat(DistrictId.Dokki, 60);

        await Assert.That(state.PolicePressure).IsEqualTo(60);
    }

    [Test]
    public async Task GameSession_SetPolicePressure_SetsAllDistricts()
    {
        using var state = new GameSession();

        state.SetPolicePressure(70);

        await Assert.That(state.DistrictHeat.GetHeat(DistrictId.Imbaba)).IsEqualTo(70);
        await Assert.That(state.DistrictHeat.GetHeat(DistrictId.Dokki)).IsEqualTo(70);
        await Assert.That(state.PolicePressure).IsEqualTo(70);
    }

    [Test]
    public async Task GameSession_CommitCrime_AddsHeatToCurrentDistrict()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        state.World.TravelTo(LocationId.Market);
        state.Player.Nutrition.Eat(MealQuality.Basic);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 40, 0, 8, 0, 10);

        var result = state.CommitCrime(attempt, new Random(42));

        if (result.PolicePressureDelta != 0)
        {
            await Assert.That(state.DistrictHeat.GetHeat(state.World.CurrentDistrict)).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task GameSession_EndDay_AppliesDistrictHeatDecay()
    {
        using var state = new GameSession();
        state.DistrictHeat.AddHeat(DistrictId.Dokki, 30);
        state.DistrictHeat.AddHeat(DistrictId.Imbaba, 30);

        state.EndDay(new Random(42));

        await Assert.That(state.DistrictHeat.GetHeat(DistrictId.Dokki)).IsLessThan(30);
        await Assert.That(state.DistrictHeat.GetHeat(DistrictId.Imbaba)).IsLessThan(30);
    }

    [Test]
    public async Task GameSession_EndDay_AppliesBleedOver()
    {
        using var state = new GameSession();
        state.DistrictHeat.AddHeat(DistrictId.Imbaba, 100);
        state.DistrictHeat.AddHeat(DistrictId.BulaqAlDakrour, 0);

        state.EndDay(new Random(42));

        await Assert.That(state.DistrictHeat.GetHeat(DistrictId.BulaqAlDakrour)).IsGreaterThan(0);
    }

    [Test]
    public async Task GameSession_EndDay_RefugeeBackground_SetsDokkiBaseline()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        state.DistrictHeat.SetHeatAll(0);

        state.EndDay(new Random(42));

        var dokkiEntry = state.DistrictHeat.Entries[DistrictId.Dokki];
        await Assert.That(dokkiEntry.BaselineHeat).IsEqualTo(10);
        await Assert.That(dokkiEntry.Heat).IsEqualTo(10);
    }

    [Test]
    public async Task GameSession_EndDay_PrisonerBackground_HalvesDecayRate()
    {
        using var state = new GameSession();
        using var control = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        control.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        state.DistrictHeat.SetHeatAll(50);
        control.DistrictHeat.SetHeatAll(50);

        state.EndDay(new Random(42));
        control.EndDay(new Random(42));

        var prisonerDokki = state.DistrictHeat.GetHeat(DistrictId.Dokki);
        var controlDokki = control.DistrictHeat.GetHeat(DistrictId.Dokki);
        await Assert.That(prisonerDokki).IsGreaterThan(controlDokki);
    }

    [Test]
    public async Task HeatDecayRates_Dokki_IsFastest()
    {
        await Assert.That(HeatDecayRates.GetDecayRate(DistrictId.Dokki)).IsEqualTo(5);
    }

    [Test]
    public async Task HeatDecayRates_Imbaba_IsModerate()
    {
        await Assert.That(HeatDecayRates.GetDecayRate(DistrictId.Imbaba)).IsEqualTo(3);
    }

    [Test]
    public async Task HeatDecayRates_SlumDistricts_AreSlow()
    {
        await Assert.That(HeatDecayRates.GetDecayRate(DistrictId.BulaqAlDakrour)).IsEqualTo(2);
        await Assert.That(HeatDecayRates.GetDecayRate(DistrictId.Shubra)).IsEqualTo(2);
        await Assert.That(HeatDecayRates.GetDecayRate(DistrictId.ArdAlLiwa)).IsEqualTo(2);
    }
}
