using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Configuration;
using Slums.Application.Content;
using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Application.Randomness;
using Slums.Core.Characters;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.World;
using Slums.Game.Screens;

namespace Slums.Game;

internal sealed class SadConsoleGame : IGame
{
    private readonly ILogger<SadConsoleGame> _logger;
    private readonly INarrativeService _narrativeService;
    private readonly ISaveGameStore _saveGameStore;
    private readonly SaveGameUseCase _saveGameUseCase;
    private readonly LoadGameUseCase _loadGameUseCase;
    private readonly IRandomSource _randomSource;
    private readonly IContentRepository _contentRepository;

    public SadConsoleGame(
        ILogger<SadConsoleGame> logger,
        INarrativeService narrativeService,
        ISaveGameStore saveGameStore,
        SaveGameUseCase saveGameUseCase,
        LoadGameUseCase loadGameUseCase,
        IRandomSource randomSource,
        IContentRepository contentRepository)
    {
        _logger = logger;
        _narrativeService = narrativeService;
        _saveGameStore = saveGameStore;
        _saveGameUseCase = saveGameUseCase;
        _loadGameUseCase = loadGameUseCase;
        _randomSource = randomSource;
        _contentRepository = contentRepository;
    }

    public void Run()
    {
        ConfigureContent();

        Settings.WindowTitle = "Slums";
        Settings.AllowWindowResize = false;

        var runtime = new GameRuntime(_narrativeService, _saveGameStore, _saveGameUseCase, _loadGameUseCase, _randomSource);

        Builder gameConfig = new Builder()
            .SetWindowSizeInCells(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight)
            .IsStartingScreenFocused(true)
            .SetStartingScreen(host => new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, runtime));

        global::SadConsole.Game.Create(gameConfig);
        global::SadConsole.Game.Instance.Run();
        global::SadConsole.Game.Instance.Dispose();
    }

    private void ConfigureContent()
    {
        var backgrounds = _contentRepository.LoadBackgrounds();
        var jobs = _contentRepository.LoadJobs();
        var locations = _contentRepository.LoadLocations();
        var randomEvents = _contentRepository.LoadRandomEvents();

        BackgroundRegistry.Configure(backgrounds);
        JobRegistry.Configure(jobs);
        WorldState.ConfigureLocations(locations);
        RandomEventRegistry.Configure(randomEvents);
        LogContentConfigured(_logger);
    }

    private static readonly Action<ILogger, Exception?> LogContentConfiguredDelegate =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, "ContentConfigured"), "Configured content from content/data.");

    private static void LogContentConfigured(ILogger logger) => LogContentConfiguredDelegate(logger, null);
}
