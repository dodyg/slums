using Slums.Core.Information;
using Slums.Core.Phone;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record PhoneMenuContext(
    bool PhoneOperational,
    bool PhoneLost,
    int CreditRemaining,
    int CreditWeekCost,
    IReadOnlyList<PhoneMessage> ActiveMessages,
    IReadOnlyList<Tip> ActiveTips,
    IReadOnlyList<Tip> UndeliveredTips,
    int CurrentDay)
{
    public static PhoneMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new PhoneMenuContext(
            gameSession.Phone.IsOperational(),
            gameSession.Phone.PhoneLost,
            gameSession.Phone.CreditRemaining,
            gameSession.Phone.CreditWeekCost,
            gameSession.PhoneMessages.GetActiveMessages(gameSession.Clock.Day),
            gameSession.Tips.GetActiveTips(gameSession.Clock.Day),
            gameSession.Tips.GetUndeliveredTips(gameSession.Clock.Day),
            gameSession.Clock.Day);
    }
}
