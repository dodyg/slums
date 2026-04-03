using FluentAssertions;
using Slums.Core.Characters;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class OwnedPetFishTankUpgradeTests
{
    [Test]
    public void HasActiveUpgrade_BetterFilter_ShouldBePermanent()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.HasActiveUpgrade(FishTankUpgradeType.BetterFilter, 1).Should().BeFalse();

        pet.PurchaseUpgrade(FishTankUpgradeType.BetterFilter, 1);

        pet.HasActiveUpgrade(FishTankUpgradeType.BetterFilter, 1).Should().BeTrue();
        pet.HasActiveUpgrade(FishTankUpgradeType.BetterFilter, 50).Should().BeTrue();
    }

    [Test]
    public void HasActiveUpgrade_Heater_ShouldBePermanent()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.HasActiveUpgrade(FishTankUpgradeType.Heater, 1).Should().BeFalse();

        pet.PurchaseUpgrade(FishTankUpgradeType.Heater, 1);

        pet.HasActiveUpgrade(FishTankUpgradeType.Heater, 1).Should().BeTrue();
        pet.HasActiveUpgrade(FishTankUpgradeType.Heater, 50).Should().BeTrue();
    }

    [Test]
    public void HasActiveUpgrade_Decorations_ShouldExpireAfterPaidWeek()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);

        pet.PurchaseUpgrade(FishTankUpgradeType.Decorations, 3);

        pet.HasActiveUpgrade(FishTankUpgradeType.Decorations, 3).Should().BeTrue();
        pet.HasActiveUpgrade(FishTankUpgradeType.Decorations, 4).Should().BeFalse();
    }

    [Test]
    public void HasActiveUpgrade_WaterConditioner_ShouldExpireAfterPaidWeek()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);

        pet.PurchaseUpgrade(FishTankUpgradeType.WaterConditioner, 2);

        pet.HasActiveUpgrade(FishTankUpgradeType.WaterConditioner, 2).Should().BeTrue();
        pet.HasActiveUpgrade(FishTankUpgradeType.WaterConditioner, 3).Should().BeFalse();
    }

    [Test]
    public void CanPurchaseUpgrade_ShouldReturnFalse_WhenAlreadyActive()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.BetterFilter, 1);

        pet.CanPurchaseUpgrade(FishTankUpgradeType.BetterFilter, 1).Should().BeFalse();
    }

    [Test]
    public void GetActiveUpgradeCount_ShouldReturn4_WhenAllActive()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.BetterFilter, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.Heater, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.Decorations, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.WaterConditioner, 1);

        pet.GetActiveUpgradeCount(1).Should().Be(4);
    }

    [Test]
    public void GetActiveUpgradeCount_ShouldReturn0_WhenNoneActive()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.GetActiveUpgradeCount(1).Should().Be(0);
    }

    [Test]
    public void Restore_ShouldRoundTripAllUpgradeFields()
    {
        var pet = OwnedPet.Restore(
            PetType.Fish,
            acquiredOnDay: 5,
            lastUpkeepPaidWeek: 3,
            hasBetterFilter: true,
            hasHeater: false,
            decorationsPaidWeek: 3,
            waterConditionerPaidWeek: 0);

        pet.Type.Should().Be(PetType.Fish);
        pet.AcquiredOnDay.Should().Be(5);
        pet.HasBetterFilter.Should().BeTrue();
        pet.HasHeater.Should().BeFalse();
        pet.HasActiveUpgrade(FishTankUpgradeType.Decorations, 3).Should().BeTrue();
        pet.HasActiveUpgrade(FishTankUpgradeType.WaterConditioner, 1).Should().BeFalse();
        pet.GetActiveUpgradeCount(3).Should().Be(2);
    }

    [Test]
    public void RecurringUpgrades_ShouldBeRenewable()
    {
        var pet = OwnedPet.Create(PetType.Fish, 1, 1);
        pet.PurchaseUpgrade(FishTankUpgradeType.Decorations, 1);

        pet.HasActiveUpgrade(FishTankUpgradeType.Decorations, 1).Should().BeTrue();
        pet.CanPurchaseUpgrade(FishTankUpgradeType.Decorations, 1).Should().BeFalse();
        pet.CanPurchaseUpgrade(FishTankUpgradeType.Decorations, 2).Should().BeTrue();

        pet.PurchaseUpgrade(FishTankUpgradeType.Decorations, 2);
        pet.HasActiveUpgrade(FishTankUpgradeType.Decorations, 2).Should().BeTrue();
    }
}
