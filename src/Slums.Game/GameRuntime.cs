using Slums.Application.Diagnostics;
using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Application.Randomness;

namespace Slums.Game;

internal sealed class GameRuntime
{
    public const int ScreenWidth = 100;
    public const int ScreenHeight = 28;

    public GameRuntime(
        INarrativeService narrativeService,
        ISaveGameStore saveGameStore,
        SaveGameUseCase saveGameUseCase,
        LoadGameUseCase loadGameUseCase,
        NewGameUseCase newGameUseCase,
        IRandomSource randomSource,
        GameMutationLogger mutationLogger)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        NarrativeService = narrativeService;
        SaveGameStore = saveGameStore;
        SaveGameUseCase = saveGameUseCase;
        LoadGameUseCase = loadGameUseCase;
        NewGameUseCase = newGameUseCase;
        SharedRandom = randomSource.SharedRandom;
        MutationLogger = mutationLogger;
    }

    public INarrativeService NarrativeService { get; }

    public ISaveGameStore SaveGameStore { get; }

    public SaveGameUseCase SaveGameUseCase { get; }

    public LoadGameUseCase LoadGameUseCase { get; }

    public NewGameUseCase NewGameUseCase { get; }

    public Random SharedRandom { get; }

    public bool QuitRequested { get; private set; }

    public void RequestQuit() => QuitRequested = true;

    public GameMutationLogger MutationLogger { get; }
}
