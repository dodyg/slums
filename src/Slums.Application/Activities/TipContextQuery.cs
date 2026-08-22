using Slums.Core.Information;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class TipContextQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetCrimeHints(GameSession gameSession)
#pragma warning restore CA1822
    {
        return GetHints(
            gameSession,
            static tip => tip.Type == TipType.CrimeWarning ||
                          (tip.Type == TipType.PoliceTip && tip.RelevantDistrict is not null),
            isWarning: true,
            includeEmergency: true);
    }

#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetWorkHints(GameSession gameSession)
#pragma warning restore CA1822
    {
        return GetHints(
            gameSession,
            static tip => tip.Type is TipType.JobLead or TipType.MarketIntel,
            isWarning: false,
            includeEmergency: false);
    }

#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetTravelHints(GameSession gameSession)
#pragma warning restore CA1822
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
