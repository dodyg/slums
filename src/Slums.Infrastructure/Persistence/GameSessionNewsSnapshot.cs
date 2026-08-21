using Slums.Core.State;
using Slums.Core.World.News;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionNewsSnapshot
{
    public IReadOnlyList<ActiveNewsFlash> ActiveFlashes { get; init; } = [];
    public IReadOnlyList<string> SeenDefinitionIds { get; init; } = [];
    public Dictionary<string, int> LastGeneratedByCategory { get; init; } = [];
    public int LastGeneratedDay { get; init; }

    public static GameSessionNewsSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionNewsSnapshot
        {
            ActiveFlashes = gameSession.News.ActiveFlashes.ToArray(),
            SeenDefinitionIds = gameSession.News.SeenDefinitionIds.ToArray(),
            LastGeneratedByCategory = gameSession.News.LastGeneratedByCategory.ToDictionary(static item => item.Key.ToString(), static item => item.Value),
            LastGeneratedDay = gameSession.News.LastGeneratedDay
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var lastGenerated = LastGeneratedByCategory
            .Where(static item => Enum.TryParse<NewsCategory>(item.Key, out _))
            .ToDictionary(static item => Enum.Parse<NewsCategory>(item.Key), static item => item.Value);
        gameSession.News.Restore(ActiveFlashes, SeenDefinitionIds, lastGenerated, LastGeneratedDay);
    }
}
