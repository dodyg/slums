namespace Slums.Infrastructure.Persistence;

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
