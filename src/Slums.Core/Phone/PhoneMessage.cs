namespace Slums.Core.Phone;

public sealed record PhoneMessage
{
    public string Id { get; init; } = PhoneMessageId.Generate();
    public PhoneMessageType Type { get; init; }
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

    public bool IsExpired(int currentDay)
    {
        return ExpiresAfterDay.HasValue && currentDay > ExpiresAfterDay.Value;
    }

    public PhoneMessage WithResponded()
    {
        return this with { Responded = true };
    }

    public PhoneMessage WithIgnored()
    {
        return this with { Ignored = true };
    }

    public PhoneMessage WithMissed()
    {
        return this with { WasMissed = true };
    }

    public PhoneMessage WithDelivered()
    {
        return this with { WasMissed = false };
    }
}
