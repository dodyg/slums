using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Territory;

internal sealed class TerritoryDynamicsCalculatorTests
{
    [Test]
    public async Task ApplyDailyDecay_ReducesTensionByDecayRate()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 50);

        var tensionBefore = territory.GetControl(DistrictId.Imbaba).Tension;
        TerritoryDynamicsCalculator.ApplyDailyDecay(territory);
        var tensionAfter = territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(tensionAfter).IsLessThan(tensionBefore);
    }

    [Test]
    public async Task ApplyDailyDecay_DoesNotReduceBelowZero()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.DowntownCairo, -10);

        TerritoryDynamicsCalculator.ApplyDailyDecay(territory);

        await Assert.That(territory.GetControl(DistrictId.DowntownCairo).Tension).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task ApplyCrimeImpact_WithApprovingFaction_IncreasesInfluence()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        TerritoryDynamicsCalculator.ApplyCrimeImpact(territory, DistrictId.Imbaba, FactionId.ImbabaCrew);
        var after = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];

        await Assert.That(after).IsGreaterThan(before);
    }

    [Test]
    public async Task ApplyCrimeImpact_WithoutApprovingFaction_IncreasesTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).Tension;
        TerritoryDynamicsCalculator.ApplyCrimeImpact(territory, DistrictId.Imbaba, null);
        var after = territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsGreaterThan(before);
    }

    [Test]
    public async Task ApplyCrimeImpact_WithoutApprovingFaction_ReducesAllInfluence()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var beforeImbaba = territory.GetControl(DistrictId.Dokki).FactionInfluence[FactionId.DokkiThugs];
        TerritoryDynamicsCalculator.ApplyCrimeImpact(territory, DistrictId.Dokki, null);
        var afterImbaba = territory.GetControl(DistrictId.Dokki).FactionInfluence[FactionId.DokkiThugs];

        await Assert.That(afterImbaba).IsLessThan(beforeImbaba);
    }

    [Test]
    public async Task ApplyHonestWorkImpact_ReducesTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 30);

        var before = territory.GetControl(DistrictId.Imbaba).Tension;
        TerritoryDynamicsCalculator.ApplyHonestWorkImpact(territory, DistrictId.Imbaba);
        var after = territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsLessThan(before);
    }

    [Test]
    public async Task ApplyPoliceCrackdown_ReducesTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 50);

        var before = territory.GetControl(DistrictId.Imbaba).Tension;
        TerritoryDynamicsCalculator.ApplyPoliceCrackdown(territory, DistrictId.Imbaba);
        var after = territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsLessThan(before);
    }

    [Test]
    public async Task ApplyPoliceCrackdown_ReducesAllFactionInfluence()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        TerritoryDynamicsCalculator.ApplyPoliceCrackdown(territory, DistrictId.Imbaba);
        var after = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];

        await Assert.That(after).IsLessThan(before);
    }

    [Test]
    public async Task ShouldTriggerPoliceCrackdown_ReturnsTrue_WhenHighTensionAndHeat()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 50);

        var result = TerritoryDynamicsCalculator.ShouldTriggerPoliceCrackdown(territory, DistrictId.Imbaba, 50);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldTriggerPoliceCrackdown_ReturnsFalse_WhenLowTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var result = TerritoryDynamicsCalculator.ShouldTriggerPoliceCrackdown(territory, DistrictId.Imbaba, 50);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ShouldTriggerPoliceCrackdown_ReturnsFalse_WhenLowHeat()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 50);

        var result = TerritoryDynamicsCalculator.ShouldTriggerPoliceCrackdown(territory, DistrictId.Imbaba, 20);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetFoodPriceModifier_ReturnsZero_WhenNormalTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var result = TerritoryDynamicsCalculator.GetFoodPriceModifier(territory, DistrictId.Imbaba);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task GetFoodPriceModifier_ReturnsPositive_WhenHighTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 40);

        var result = TerritoryDynamicsCalculator.GetFoodPriceModifier(territory, DistrictId.Imbaba);

        await Assert.That(result).IsGreaterThan(0);
    }

    [Test]
    public async Task GetFoodPriceModifier_ReturnsLargest_WhenDangerousTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 40);

        var high = TerritoryDynamicsCalculator.GetFoodPriceModifier(territory, DistrictId.Imbaba);

        territory.ModifyTension(DistrictId.Imbaba, 50);

        var dangerous = TerritoryDynamicsCalculator.GetFoodPriceModifier(territory, DistrictId.Imbaba);

        await Assert.That(dangerous).IsGreaterThan(high);
    }

    [Test]
    public async Task IsCrimeBlocked_ReturnsTrue_WhenDangerousTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 60);

        var result = TerritoryDynamicsCalculator.IsCrimeBlocked(territory, DistrictId.Imbaba);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsCrimeBlocked_ReturnsFalse_WhenNormalTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var result = TerritoryDynamicsCalculator.IsCrimeBlocked(territory, DistrictId.Imbaba);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsNonControllingCrimeBlocked_ReturnsTrue_WhenHighTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);
        territory.ModifyTension(DistrictId.Imbaba, 40);

        var result = TerritoryDynamicsCalculator.IsNonControllingCrimeBlocked(territory, DistrictId.Imbaba);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldTriggerConflictEvent_ReturnsFalse_WhenNormalTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

#pragma warning disable CA5394
        var result = TerritoryDynamicsCalculator.ShouldTriggerConflictEvent(territory, DistrictId.Imbaba, new Random(1));
#pragma warning restore CA5394

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task DetectTerritoryFlip_ReturnsFaction_WhenControlChanges()
    {
        var before = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60, [FactionId.DokkiThugs] = 10 },
            20, 0);

        var after = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 30, [FactionId.DokkiThugs] = 55 },
            15, 0);

        var result = TerritoryDynamicsCalculator.DetectTerritoryFlip(before, after);

        await Assert.That(result).IsEqualTo(FactionId.DokkiThugs);
    }

    [Test]
    public async Task DetectTerritoryFlip_ReturnsNull_WhenControlUnchanged()
    {
        var before = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60, [FactionId.DokkiThugs] = 10 },
            20, 0);

        var after = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 55, [FactionId.DokkiThugs] = 10 },
            18, 0);

        var result = TerritoryDynamicsCalculator.DetectTerritoryFlip(before, after);

        await Assert.That(result).IsNull();
    }
}
