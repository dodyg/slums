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
}
