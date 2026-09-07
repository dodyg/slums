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
        SaveGameSnapshotValidator.Validate(snapshot, problems);

        if (problems.Count > 0)
        {
            throw new InvalidDataException("Save data validation failed: " + string.Join("; ", problems));
        }
    }
}
