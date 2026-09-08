using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Configuration;
using Slums.Application.Content;
using Slums.Application.Diagnostics;
using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Core.Characters;
using Slums.Core.Content;
using Slums.Core.Diagnostics;
using Slums.Core.Endings;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.Robotics;
using Slums.Core.World;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Core.World.News;
using Slums.Game.Content;
using Slums.Game.Screens;
using Slums.Infrastructure.Content;
using Slums.Narrative.Ink;

namespace Slums.Game;

internal sealed class SadConsoleGame : IGame
{
    private readonly ILogger<SadConsoleGame> _logger;
    private readonly INarrativeService _narrativeService;
    private readonly ISaveGameStore _saveGameStore;
    private readonly SaveGameUseCase _saveGameUseCase;
    private readonly LoadGameUseCase _loadGameUseCase;
    private readonly NewGameUseCase _newGameUseCase;
    private readonly IGameContentCatalogProvider _contentCatalogProvider;
    private readonly IContentBootstrapper _contentBootstrapper;
    private readonly GameMutationLogger _mutationLogger;
    private GameContentCatalog? _contentCatalog;

    public SadConsoleGame(
        ILogger<SadConsoleGame> logger,
        INarrativeService narrativeService,
        ISaveGameStore saveGameStore,
        SaveGameUseCase saveGameUseCase,
        LoadGameUseCase loadGameUseCase,
        NewGameUseCase newGameUseCase,
        IGameContentCatalogProvider contentCatalogProvider,
        IContentBootstrapper contentBootstrapper,
        GameMutationLogger mutationLogger)
    {
        _logger = logger;
        _narrativeService = narrativeService;
        _saveGameStore = saveGameStore;
        _saveGameUseCase = saveGameUseCase;
        _loadGameUseCase = loadGameUseCase;
        _newGameUseCase = newGameUseCase;
        _contentCatalogProvider = contentCatalogProvider;
        _contentBootstrapper = contentBootstrapper;
        _mutationLogger = mutationLogger;
    }

    public void Run()
    {
        ConfigureContent();
        _ = _contentCatalog ?? throw new InvalidOperationException("Content bootstrap completed without a catalog.");

        Settings.WindowTitle = "Slums";
        Settings.AllowWindowResize = false;

        var runtime = new GameRuntime(
            _narrativeService,
            _saveGameStore,
            _saveGameUseCase,
            _loadGameUseCase,
            _newGameUseCase,
            _mutationLogger);

        Builder gameConfig = new Builder()
            .SetWindowSizeInCells(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight)
            .IsStartingScreenFocused(true)
            .SetStartingScreen(host => new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, runtime));

        global::SadConsole.Game.Create(gameConfig);
        global::SadConsole.GameHost.Instance.FrameUpdate += StopWhenQuitRequested;
        global::SadConsole.Game.Instance.Run();
        global::SadConsole.GameHost.Instance.FrameUpdate -= StopWhenQuitRequested;
        global::SadConsole.Game.Instance.Dispose();

        void StopWhenQuitRequested(object? sender, GameHost host)
        {
            if (runtime.QuitRequested)
            {
                host.Dispose();
            }
        }
    }

    private void ConfigureContent()
    {
        _contentCatalog = _contentBootstrapper.Bootstrap();
    }
}
