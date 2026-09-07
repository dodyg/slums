using Slums.Application.Randomness;
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

    public NewGameUseCase(IRandomSource randomSource, GameContentCatalog? contentCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        _randomSource = randomSource;
        _contentCatalog = contentCatalog;
    }

    /// <summary>Creates a new game session backed by the shared random source.</summary>
    public GameSession Execute()
    {
        return new GameSession(_randomSource.SharedRandom, _contentCatalog);
    }
}
