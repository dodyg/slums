namespace Slums.Core.Community;

public sealed record CommunityEventDefinition(
    CommunityEventId Id,
    string Name,
    string Description,
    int TimeCostMinutes,
    int MoneyCost,
    int StressChange,
    int TrustGainCount,
    int TrustGainAmount,
    bool ProvidesFoodAccess,
    bool ProvidesInformationTips,
    bool RequiresFriday,
    bool RequiresRamadan,
    bool RequiresNpcInvitation,
    bool IsSeasonal,
    bool HasPickpocketRisk)
{
    /// <summary>Game-clock days on which a seasonal event can be attended.</summary>
    public IReadOnlyList<int> SeasonalAnchorDays { get; init; } = [];
}
