using Slums.Core.State;

namespace Slums.Application.News;

public sealed class AcknowledgeNewsCommand
{
    #pragma warning disable CA1822
    public (bool Success, string Message) Execute(GameSession gameSession, string newsId)
    #pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsId);
        if (!gameSession.ActiveNews.Any(flash => flash.DefinitionId == newsId))
        {
            return (false, "That news flash is no longer active.");
        }

        gameSession.News.Acknowledge(newsId);
        return (true, "News marked as read.");
    }
}
