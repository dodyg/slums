using Slums.Core.Content;

namespace Slums.Game.Content;

/// <summary>Loads and validates repository content once during startup, then publishes the catalog.</summary>
internal interface IContentBootstrapper
{
    /// <summary>Loads, validates, and publishes the content catalog; throws when any step fails.</summary>
    public GameContentCatalog Bootstrap();
}
