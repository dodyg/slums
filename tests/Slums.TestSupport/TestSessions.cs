using Slums.Core.Content;
using Slums.Core.State;

namespace Slums.TestSupport;

/// <summary>
/// Factory for game sessions in tests. Every session receives the shared immutable
/// <see cref="TestContent.Catalog"/> unless a caller supplies its own catalog, so sessions
/// never depend on process-global registry state and can be created in parallel.
/// </summary>
public static class TestSessions
{
    /// <summary>Creates a new session with the shared content catalog and an optional shared random source.</summary>
    public static GameSession Create(Random? sharedRandom = null, GameContentCatalog? contentCatalog = null)
    {
        return new GameSession(sharedRandom, contentCatalog ?? TestContent.Catalog);
    }
}
