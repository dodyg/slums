using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class FoodPreservationSnapshotTests
{
    [Test]
    public async Task Snapshot_ShouldPreserveFoodAndPreservedMeals()
    {
        var original = new GameSession();
        original.Player.Household.SetFoodStockpile(4);
        original.Player.Household.SetPreservedMealUnits(3);

        var restored = GameSessionSnapshot.Capture(original).Restore();

        await Assert.That(restored.Player.Household.FoodStockpile).IsEqualTo(4);
        await Assert.That(restored.Player.Household.PreservedMealUnits).IsEqualTo(3);
    }

    [Test]
    public async Task Snapshot_ShouldPreserveWaterPumpCondition()
    {
        var original = new GameSession();
        original.Technology.RepairWaterPump(20);

        var restored = GameSessionSnapshot.Capture(original).Restore();

        await Assert.That(restored.Technology.WaterPumpCondition).IsEqualTo(80);
    }
}
