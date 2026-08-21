namespace Slums.Core.World.News;

public sealed record ActiveNewsFlash
{
    public string DefinitionId { get; init; } = string.Empty;
    public int StartDay { get; init; }
    public int ExpiryDay { get; init; }
    public bool Acknowledged { get; init; }
    public string? UsedResponseId { get; init; }

    public bool IsActive(int currentDay) => currentDay <= ExpiryDay;
}
