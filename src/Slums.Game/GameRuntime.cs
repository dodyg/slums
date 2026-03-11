using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Application.Randomness;

namespace Slums.Game;

internal sealed class GameRuntime
{
    public const int ScreenWidth = 80;
    public const int ScreenHeight = 25;

    public GameRuntime(
        INarrativeService narrativeService,
        ISaveGameStore saveGameStore,
        SaveGameUseCase saveGameUseCase,
        LoadGameUseCase loadGameUseCase,
        IRandomSource randomSource)
    {
        NarrativeService = narrativeService;
        SaveGameStore = saveGameStore;
        SaveGameUseCase = saveGameUseCase;
        LoadGameUseCase = loadGameUseCase;
        RandomSource = randomSource;
    }

    public INarrativeService NarrativeService { get; }

    public ISaveGameStore SaveGameStore { get; }

    public SaveGameUseCase SaveGameUseCase { get; }

    public LoadGameUseCase LoadGameUseCase { get; }

    public IRandomSource RandomSource { get; }
}