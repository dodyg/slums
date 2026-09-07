using Slums.Core.Information;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class TipContextQuery
{
    public IReadOnlyList<TipContextHint> GetCrimeHints(GameSession gameSession)
    {
        return GetHints(
            gameSession,
            static tip => tip.Type == TipType.CrimeWarning ||
                          (tip.Type == TipType.PoliceTip && tip.RelevantDistrict is not null),
            isWarning: true,
            includeEmergency: true);
    }

    public IReadOnlyList<TipContextHint> GetWorkHints(GameSession gameSession)
    {
        return GetHints(
            gameSession,
            static tip => tip.Type is TipType.JobLead or TipType.MarketIntel,
            isWarning: false,
            includeEmergency: false);
    }

    public IReadOnlyList<TipContextHint> GetTravelHints(GameSession gameSession)
    {
        return GetHints(
            gameSession,
            static tip => tip.Type == TipType.PoliceTip && tip.RelevantDistrict is not null,
            isWarning: true,
            includeEmergency: true);
    }

    private static TipContextHint[] GetHints(
        GameSession gameSession,
        Func<Tip, bool> isRelevant,
        bool isWarning,
        bool includeEmergency)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(isRelevant);

        return gameSession.Tips
            .GetActiveTips(gameSession.Clock.Day)
            .Where(tip => !tip.Ignored && isRelevant(tip))
            .Select(tip => new TipContextHint(
                tip.Content,
                isWarning,
                includeEmergency && tip.IsEmergency))
            .ToArray();
    }
}

public sealed record TipContextHint(string Content, bool IsWarning, bool IsEmergency);
