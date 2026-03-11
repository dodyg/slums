using Slums.Core.World;

namespace Slums.Core.Relationships;

public static class NpcRegistry
{
    public static string GetName(NpcId npcId) => npcId switch
    {
        NpcId.LandlordHajjMahmoud => "Hajj Mahmoud",
        NpcId.FixerUmmKarim => "Umm Karim",
        NpcId.OfficerKhalid => "Officer Khalid",
        _ => throw new ArgumentOutOfRangeException(nameof(npcId))
    };

    public static IReadOnlyList<NpcId> GetReachableNpcs(LocationId locationId, int policePressure)
    {
        var npcs = new List<NpcId>();

        if (locationId == LocationId.Home)
        {
            npcs.Add(NpcId.LandlordHajjMahmoud);
        }

        if (locationId == LocationId.Market || locationId == LocationId.Square)
        {
            npcs.Add(NpcId.FixerUmmKarim);
        }

        if (locationId == LocationId.Square || policePressure >= 50)
        {
            npcs.Add(NpcId.OfficerKhalid);
        }

        return npcs;
    }

    public static string GetConversationKnot(NpcId npcId, RelationshipState relationshipState, int policePressure)
    {
        ArgumentNullException.ThrowIfNull(relationshipState);

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when relationshipState.GetNpcRelationship(npcId).Trust <= -15 => "landlord_rent_negotiation_hostile",
            NpcId.LandlordHajjMahmoud when relationshipState.GetNpcRelationship(npcId).Trust >= 15 => "landlord_rent_negotiation_warm",
            NpcId.LandlordHajjMahmoud => "landlord_rent_negotiation",
            NpcId.FixerUmmKarim when relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "fixer_repeat_contact",
            NpcId.FixerUmmKarim => "fixer_first_contact",
            NpcId.OfficerKhalid when policePressure >= 70 => "officer_checkpoint_hot",
            NpcId.OfficerKhalid => "officer_checkpoint",
            _ => throw new ArgumentOutOfRangeException(nameof(npcId))
        };
    }
}