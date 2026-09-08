using Slums.Application.Randomness;
using Slums.Application.Content;
using Slums.Core.Content;
using Slums.Core.State;

namespace Slums.Application.Persistence;

/// <summary>
/// Creates a fresh <see cref="GameSession"/> for a new playthrough so the UI never
/// constructs domain sessions directly.
/// </summary>
public sealed class NewGameUseCase
{
    private readonly IRandomSource _randomSource;
    private readonly GameContentCatalog? _contentCatalog;
    private readonly IGameContentCatalogProvider? _contentCatalogProvider;

    public NewGameUseCase(
        IRandomSource randomSource,
        GameContentCatalog? contentCatalog = null,
        IGameContentCatalogProvider? contentCatalogProvider = null)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        _randomSource = randomSource;
        _contentCatalog = contentCatalog;
        _contentCatalogProvider = contentCatalogProvider;
    }

    /// <summary>Creates a new game session backed by the shared random source.</summary>
    public GameSession Execute()
    {
        var contentCatalog = _contentCatalog
            ?? _contentCatalogProvider?.Current
            ?? (_contentCatalogProvider is null ? GameContentCatalog.FromConfiguredRegistries() : null);
        if (contentCatalog is null)
        {
            throw new InvalidOperationException("Content must be bootstrapped before starting a new game.");
        }

        return new GameSession(_randomSource.SharedRandom, contentCatalog);
    }
}
