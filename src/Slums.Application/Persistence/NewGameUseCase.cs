using Slums.Application.Randomness;
using Slums.Application.Content;
using Microsoft.Extensions.Logging;
using Slums.Core.Content;
using Slums.Core.Diagnostics;
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
    private readonly ILogger<NewGameUseCase>? _logger;

    public NewGameUseCase(
        IRandomSource randomSource,
        GameContentCatalog? contentCatalog = null,
        IGameContentCatalogProvider? contentCatalogProvider = null,
        ILogger<NewGameUseCase>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        _randomSource = randomSource;
        _contentCatalog = contentCatalog;
        _contentCatalogProvider = contentCatalogProvider;
        _logger = logger;
    }

    /// <summary>Creates a new game session backed by the shared random source.</summary>
    public GameSession Execute()
    {
        var contentCatalog = _contentCatalog ?? _contentCatalogProvider?.Current;
        if (contentCatalog is null)
        {
            throw new InvalidOperationException("Content must be bootstrapped before starting a new game.");
        }

        var session = new GameSession(_randomSource.SharedRandom, contentCatalog);
        LogNewGameCreated(_logger, session.RunId, session.ContentCatalog.Investments.Count);
        return session;
    }

    private static readonly Action<ILogger, Guid, int, Exception?> LogNewGameCreatedDelegate =
        LoggerMessage.Define<Guid, int>(LogLevel.Information, new EventId(LogEvents.NewGameCreated, "NewGameCreated"), "Created new game run {RunId} with {InvestmentDefinitionCount} investment definitions.");

    private static void LogNewGameCreated(ILogger<NewGameUseCase>? logger, Guid runId, int investmentDefinitionCount)
    {
        if (logger is not null)
        {
            LogNewGameCreatedDelegate(logger, runId, investmentDefinitionCount, null);
        }
    }
}
