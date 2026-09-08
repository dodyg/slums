using Slums.Core.Content;

namespace Slums.Application.Content;

/// <summary>Process-local handoff from content bootstrap to the game use cases.</summary>
public sealed class GameContentCatalogProvider : IGameContentCatalogProvider
{
    public GameContentCatalog? Current { get; private set; }

    public void Publish(GameContentCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        Current = catalog;
    }
}
