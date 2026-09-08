using Slums.Core.Content;
using Slums.Core.State;

namespace Slums.Application.News;

public sealed record NewsMenuContext(
    int CurrentDay,
    IReadOnlyList<Slums.Core.World.News.NewsFlashDefinition> ActiveNews,
    IReadOnlyList<Slums.Core.World.News.ActiveNewsFlash> ActiveStates,
    GameContentCatalog Catalog)
{
    public static NewsMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new NewsMenuContext(gameSession.Clock.Day, gameSession.GetActiveNewsDefinitions(), gameSession.ActiveNews, gameSession.ContentCatalog);
    }
}
