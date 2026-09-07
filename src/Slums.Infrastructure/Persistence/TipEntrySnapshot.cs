namespace Slums.Infrastructure.Persistence;

public sealed record TipEntrySnapshot
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string Source { get; init; } = "";
    public string Content { get; init; } = "";
    public int DayGenerated { get; init; }
    public int ExpiresAfterDay { get; init; }
    public string? RelevantDistrict { get; init; }
    public bool Acknowledged { get; init; }
    public bool Ignored { get; init; }
    public bool Delivered { get; init; }
    public bool IsEmergency { get; init; }
}
