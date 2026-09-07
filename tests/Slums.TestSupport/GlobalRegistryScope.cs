using Slums.Core.Inventory;
using Slums.Core.World.News;

namespace Slums.TestSupport;

public sealed class GlobalRegistryScope : IDisposable
{
    private readonly IReadOnlyList<ItemDefinition> _items = ItemRegistry.All.ToArray();
    private readonly IReadOnlyList<NewsFlashDefinition> _news = NewsRegistry.All.ToArray();

    public void Dispose()
    {
        ItemRegistry.Configure(_items);
        NewsRegistry.Configure(_news);
    }
}
