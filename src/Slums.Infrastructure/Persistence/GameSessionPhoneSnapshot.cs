using Slums.Core.Phone;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionPhoneSnapshot
{
    public bool HasPhone { get; init; } = true;
    public int CreditRemaining { get; init; } = 7;
    public int DaysSinceCreditRefill { get; init; }
    public bool PhoneLost { get; init; }
    public int? PhoneLostDay { get; init; }
    public bool PhoneRecovered { get; init; }
    public IReadOnlyList<PhoneMessageSnapshot> Messages { get; init; } = [];

    public static GameSessionPhoneSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionPhoneSnapshot
        {
            HasPhone = gameSession.Phone.HasPhone,
            CreditRemaining = gameSession.Phone.CreditRemaining,
            DaysSinceCreditRefill = gameSession.Phone.DaysSinceCreditRefill,
            PhoneLost = gameSession.Phone.PhoneLost,
            PhoneLostDay = gameSession.Phone.PhoneLostDay,
            PhoneRecovered = gameSession.Phone.PhoneRecovered,
            Messages = gameSession.PhoneMessages.Inbox.Select(static m => new PhoneMessageSnapshot
            {
                Id = m.Id,
                Type = m.Type.ToString(),
                Sender = m.Sender,
                SenderNpcId = m.SenderNpcId,
                Content = m.Content,
                DayReceived = m.DayReceived,
                ExpiresAfterDay = m.ExpiresAfterDay,
                RequiresResponse = m.RequiresResponse,
                ResponseTimeCost = m.ResponseTimeCost,
                ResponseMoneyCost = m.ResponseMoneyCost,
                Responded = m.Responded,
                Ignored = m.Ignored,
                WasMissed = m.WasMissed
            }).ToArray()
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        gameSession.RestorePhoneState(
            HasPhone, CreditRemaining, DaysSinceCreditRefill,
            PhoneLost, PhoneLostDay, PhoneRecovered);

        var messages = Messages.Select(static s => new PhoneMessage
        {
            Id = s.Id,
            Type = Enum.Parse<PhoneMessageType>(s.Type),
            Sender = s.Sender,
            SenderNpcId = s.SenderNpcId,
            Content = s.Content,
            DayReceived = s.DayReceived,
            ExpiresAfterDay = s.ExpiresAfterDay,
            RequiresResponse = s.RequiresResponse,
            ResponseTimeCost = s.ResponseTimeCost,
            ResponseMoneyCost = s.ResponseMoneyCost,
            Responded = s.Responded,
            Ignored = s.Ignored,
            WasMissed = s.WasMissed
        });

        gameSession.RestorePhoneMessages(messages);
    }
}

public sealed record PhoneMessageSnapshot
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string Sender { get; init; } = "";
    public string SenderNpcId { get; init; } = "";
    public string Content { get; init; } = "";
    public int DayReceived { get; init; }
    public int? ExpiresAfterDay { get; init; }
    public bool RequiresResponse { get; init; }
    public int ResponseTimeCost { get; init; }
    public int ResponseMoneyCost { get; init; }
    public bool Responded { get; init; }
    public bool Ignored { get; init; }
    public bool WasMissed { get; init; }
}
