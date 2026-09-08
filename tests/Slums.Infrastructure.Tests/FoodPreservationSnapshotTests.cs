using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Infrastructure.Tests;

internal sealed class FoodPreservationSnapshotTests
{
    [Test]
    public async Task Snapshot_ShouldPreserveFoodAndPreservedMeals()
    {
        var original = TestSessions.Create();
        original.Player.Household.SetFoodStockpile(4);
        original.Player.Household.SetPreservedMealUnits(3);

        var restored = GameSessionSnapshot.Capture(original).Restore(TestContent.Catalog);

        await Assert.That(restored.Player.Household.FoodStockpile).IsEqualTo(4);
        await Assert.That(restored.Player.Household.PreservedMealUnits).IsEqualTo(3);
    }

    [Test]
    public async Task Snapshot_ShouldPreserveWaterPumpCondition()
    {
        var original = TestSessions.Create();
        original.Technology.RepairWaterPump(20);

        var restored = GameSessionSnapshot.Capture(original).Restore(TestContent.Catalog);

        await Assert.That(restored.Technology.WaterPumpCondition).IsEqualTo(80);
    }
}
