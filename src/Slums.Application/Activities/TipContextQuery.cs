using Slums.Core.Information;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class TipContextQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetCrimeHints(GameSession gameSession)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var day = gameSession.Clock.Day;
        var hints = new List<TipContextHint>();

        foreach (var tip in gameSession.Tips.GetActiveTips(day))
        {
            if (tip.Type == TipType.CrimeWarning && !tip.Ignored)
            {
                hints.Add(new TipContextHint(tip.Content, true, tip.IsEmergency));
            }

            if (tip.Type == TipType.PoliceTip && !tip.Ignored && tip.RelevantDistrict is not null)
            {
                hints.Add(new TipContextHint(tip.Content, true, tip.IsEmergency));
            }
        }

        return hints;
    }

#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetWorkHints(GameSession gameSession)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var day = gameSession.Clock.Day;
        var hints = new List<TipContextHint>();

        foreach (var tip in gameSession.Tips.GetActiveTips(day))
        {
            if (tip.Type == TipType.JobLead && !tip.Ignored)
            {
                hints.Add(new TipContextHint(tip.Content, false, false));
            }

            if (tip.Type == TipType.MarketIntel && !tip.Ignored)
            {
                hints.Add(new TipContextHint(tip.Content, false, false));
            }
        }

        return hints;
    }

#pragma warning disable CA1822
    public IReadOnlyList<TipContextHint> GetTravelHints(GameSession gameSession)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var day = gameSession.Clock.Day;
        var hints = new List<TipContextHint>();

        foreach (var tip in gameSession.Tips.GetActiveTips(day))
        {
            if (tip.Type == TipType.PoliceTip && !tip.Ignored && tip.RelevantDistrict is not null)
            {
                hints.Add(new TipContextHint(tip.Content, true, tip.IsEmergency));
            }
        }

        return hints;
    }
}

public sealed record TipContextHint(string Content, bool IsWarning, bool IsEmergency);
