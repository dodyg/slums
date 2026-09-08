using Slums.Core.Content;

namespace Slums.Application.Content;

/// <summary>Supplies the validated content catalog prepared during application startup.</summary>
public interface IGameContentCatalogProvider
{
    /// <summary>Gets the initialized catalog, or <c>null</c> before bootstrap completes.</summary>
    public GameContentCatalog? Current { get; }

    /// <summary>Publishes the validated catalog for use-case consumers.</summary>
    public void Publish(GameContentCatalog catalog);
}
