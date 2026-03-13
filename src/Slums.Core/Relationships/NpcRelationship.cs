namespace Slums.Core.Relationships;

public sealed record NpcRelationship(
	NpcId NpcId,
	int Trust = 0,
	int LastSeenDay = 0,
	int LastFavorDay = 0,
	int LastRefusalDay = 0,
	bool HasUnpaidDebt = false,
	bool WasEmbarrassed = false,
	bool WasHelped = false,
	int RecentContactCount = 0)
{
	public IReadOnlySet<string> SeenConversationKnots { get; init; } = new HashSet<string>();
}