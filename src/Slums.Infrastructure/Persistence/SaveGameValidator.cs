using System.Linq;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

/// <summary>
/// Validates a deserialized <see cref="GameSessionSnapshot"/> before its state is restored.
/// Broken saves (out-of-range values, unknown ids) fail as corrupt instead of restoring
/// inconsistent state.
/// </summary>
public static class SaveGameValidator
{
    public static void Validate(GameSessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var problems = new List<string>();

        if (snapshot.Clock.Day < 1)
        {
            problems.Add($"day {snapshot.Clock.Day} is below 1");
        }

        if (snapshot.Clock.Hour is < 0 or > 23)
        {
            problems.Add($"hour {snapshot.Clock.Hour} is outside 0..23");
        }

        if (snapshot.Clock.Minute is < 0 or > 59)
        {
            problems.Add($"minute {snapshot.Clock.Minute} is outside 0..59");
        }

        if (snapshot.Player.Money < 0)
        {
            problems.Add($"money {snapshot.Player.Money} is negative");
        }

        if (!IsPercentage(snapshot.Player.Energy))
        {
            problems.Add($"energy {snapshot.Player.Energy} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.Health))
        {
            problems.Add($"health {snapshot.Player.Health} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.Stress))
        {
            problems.Add($"stress {snapshot.Player.Stress} is outside 0..100");
        }

        if (snapshot.Player.Satiety is < 0 or > 100)
        {
            problems.Add($"satiety {snapshot.Player.Satiety} is outside 0..100");
        }

        if (!IsPercentage(snapshot.Player.MotherHealth))
        {
            problems.Add($"mother health {snapshot.Player.MotherHealth} is outside 0..100");
        }

        if (snapshot.Player.FoodStockpile < 0)
        {
            problems.Add($"food stockpile {snapshot.Player.FoodStockpile} is negative");
        }

        if (!LocationId.All.Any(location => location.Value == snapshot.World.CurrentLocationId))
        {
            problems.Add($"current location '{snapshot.World.CurrentLocationId}' is not a declared location");
        }

        if (snapshot.Crime.PolicePressure is < 0 or > 100)
        {
            problems.Add($"police pressure {snapshot.Crime.PolicePressure} is outside 0..100");
        }

        if (problems.Count > 0)
        {
            throw new InvalidDataException("Save data validation failed: " + string.Join("; ", problems));
        }
    }

    private static bool IsPercentage(int value) => value is >= 0 and <= 100;
}
