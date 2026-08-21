namespace Slums.Core.Relationships;

public static class NpcScheduleRegistry
{
    private static IReadOnlyList<NpcScheduleDefinition> _definitions = [];

    public static IReadOnlyList<NpcScheduleDefinition> All => _definitions;

    public static void Configure(IEnumerable<NpcScheduleDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = definitions.ToArray();
    }
}
