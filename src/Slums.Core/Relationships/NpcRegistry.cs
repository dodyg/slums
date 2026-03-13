using Slums.Core.World;

namespace Slums.Core.Relationships;

public static class NpcRegistry
{
    public static string GetName(NpcId npcId) => npcId switch
    {
        NpcId.LandlordHajjMahmoud => "Hajj Mahmoud",
        NpcId.FixerUmmKarim => "Umm Karim",
        NpcId.OfficerKhalid => "Officer Khalid",
        NpcId.NeighborMona => "Mona",
        NpcId.NurseSalma => "Nurse Salma",
        NpcId.WorkshopBossAbuSamir => "Abu Samir",
        NpcId.CafeOwnerNadia => "Nadia",
        NpcId.FenceHanan => "Hanan",
        NpcId.RunnerYoussef => "Youssef",
        NpcId.PharmacistMariam => "Mariam",
        NpcId.DispatcherSafaa => "Safaa",
        NpcId.LaundryOwnerIman => "Iman",
        _ => throw new ArgumentOutOfRangeException(nameof(npcId))
    };

    public static IReadOnlyList<NpcId> GetReachableNpcs(LocationId locationId, int policePressure)
    {
        var npcs = new List<NpcId>();

        if (locationId == LocationId.Home)
        {
            npcs.Add(NpcId.LandlordHajjMahmoud);
            npcs.Add(NpcId.NeighborMona);
        }

        if (locationId == LocationId.Market || locationId == LocationId.Square)
        {
            npcs.Add(NpcId.FixerUmmKarim);
        }

        if (locationId == LocationId.Market)
        {
            npcs.Add(NpcId.FenceHanan);
        }

        if (locationId == LocationId.Clinic)
        {
            npcs.Add(NpcId.NurseSalma);
        }

        if (locationId == LocationId.Workshop)
        {
            npcs.Add(NpcId.WorkshopBossAbuSamir);
        }

        if (locationId == LocationId.Cafe)
        {
            npcs.Add(NpcId.CafeOwnerNadia);
        }

        if (locationId == LocationId.Square)
        {
            npcs.Add(NpcId.RunnerYoussef);
        }

        if (locationId == LocationId.Pharmacy)
        {
            npcs.Add(NpcId.PharmacistMariam);
        }

        if (locationId == LocationId.Depot)
        {
            npcs.Add(NpcId.DispatcherSafaa);
        }

        if (locationId == LocationId.Laundry)
        {
            npcs.Add(NpcId.LaundryOwnerIman);
        }

        if (locationId == LocationId.Square || policePressure >= 50)
        {
            npcs.Add(NpcId.OfficerKhalid);
        }

        return npcs;
    }

    public static string GetConversationKnot(NpcId npcId, RelationshipState relationshipState, int policePressure)
    {
        return GetConversationKnot(npcId, relationshipState, policePressure, currentDay: 0, honestShiftsCompleted: 0, crimesCommitted: 0, currentMoney: 100, motherHealth: 70);
    }

    public static string GetConversationKnot(NpcId npcId, RelationshipState relationshipState, int policePressure, int currentDay, int honestShiftsCompleted, int crimesCommitted, int currentMoney = 100, int motherHealth = 70)
    {
        ArgumentNullException.ThrowIfNull(relationshipState);

        var relationship = relationshipState.GetNpcRelationship(npcId);
        var context = ConversationPoolRegistry.DetermineContext(npcId, relationship, policePressure, currentDay, honestShiftsCompleted, crimesCommitted, currentMoney, motherHealth);
        var pool = ConversationPoolRegistry.GetConversationPool(npcId, context);
        var seenKnots = relationship.SeenConversationKnots;
        var available = pool.Where(k => !seenKnots.Contains(k)).ToList();

#pragma warning disable CA5394 // Random is sufficient for gameplay mechanics
        return available.Count > 0
            ? available[Random.Shared.Next(available.Count)]
            : pool[Random.Shared.Next(pool.Count)];
#pragma warning restore CA5394
    }
}