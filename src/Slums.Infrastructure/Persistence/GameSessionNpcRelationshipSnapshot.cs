using System.Collections.ObjectModel;
using Slums.Core.Relationships;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionNpcRelationshipSnapshot
{
    public int Trust { get; init; }

    public int LastSeenDay { get; init; }

    public int LastFavorDay { get; init; }

    public int LastRefusalDay { get; init; }

    public bool HasUnpaidDebt { get; init; }

    public bool WasEmbarrassed { get; init; }

    public bool WasHelped { get; init; }

    public int RecentContactCount { get; init; }

    public Collection<string> SeenConversationKnots { get; init; } = [];

    public static GameSessionNpcRelationshipSnapshot Capture(NpcRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return new GameSessionNpcRelationshipSnapshot
        {
            Trust = relationship.Trust,
            LastSeenDay = relationship.LastSeenDay,
            LastFavorDay = relationship.LastFavorDay,
            LastRefusalDay = relationship.LastRefusalDay,
            HasUnpaidDebt = relationship.HasUnpaidDebt,
            WasEmbarrassed = relationship.WasEmbarrassed,
            WasHelped = relationship.WasHelped,
            RecentContactCount = relationship.RecentContactCount,
            SeenConversationKnots = new Collection<string>([.. relationship.SeenConversationKnots])
        };
    }
}
