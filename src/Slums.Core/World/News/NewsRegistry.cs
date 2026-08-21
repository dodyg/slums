namespace Slums.Core.World.News;

public static class NewsRegistry
{
    private static IReadOnlyList<NewsFlashDefinition> _definitions = [];

    public static IReadOnlyList<NewsFlashDefinition> All => _definitions;

    public static void Configure(IEnumerable<NewsFlashDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = definitions.ToArray();
    }

    public static NewsFlashDefinition? GetById(string id)
    {
        return _definitions.FirstOrDefault(definition => definition.Id == id);
    }
}
