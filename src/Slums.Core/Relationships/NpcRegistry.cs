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
        return GetConversationKnot(npcId, relationshipState, policePressure, currentDay: 0, honestShiftsCompleted: 0, crimesCommitted: 0);
    }

    public static string GetConversationKnot(NpcId npcId, RelationshipState relationshipState, int policePressure, int currentDay, int honestShiftsCompleted, int crimesCommitted)
    {
        ArgumentNullException.ThrowIfNull(relationshipState);

        var relationship = relationshipState.GetNpcRelationship(npcId);
        var maintainingDoubleLife = honestShiftsCompleted >= 3 && crimesCommitted > 0;

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when relationship.Trust <= -15 => "landlord_rent_negotiation_hostile",
            NpcId.LandlordHajjMahmoud when relationship.Trust >= 15 => "landlord_rent_negotiation_warm",
            NpcId.LandlordHajjMahmoud => "landlord_rent_negotiation",
            NpcId.FixerUmmKarim when relationship.LastRefusalDay > 0 && currentDay - relationship.LastRefusalDay <= 3 => "fixer_recent_refusal",
            NpcId.FixerUmmKarim when relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "fixer_repeat_contact",
            NpcId.FixerUmmKarim => "fixer_first_contact",
            NpcId.OfficerKhalid when policePressure >= 70 => "officer_checkpoint_hot",
            NpcId.OfficerKhalid => "officer_checkpoint",
            NpcId.NeighborMona when relationship.WasHelped => "neighbor_mona_helped",
            NpcId.NeighborMona when relationship.Trust >= 15 => "neighbor_mona_warm",
            NpcId.NeighborMona => "neighbor_mona",
            NpcId.NurseSalma when relationship.HasUnpaidDebt => "nurse_salma_debt",
            NpcId.NurseSalma when maintainingDoubleLife => "nurse_salma_suspicious",
            NpcId.NurseSalma when relationship.Trust >= 15 => "nurse_salma_warm",
            NpcId.NurseSalma => "nurse_salma",
            NpcId.WorkshopBossAbuSamir when relationship.WasEmbarrassed => "abu_samir_embarrassed",
            NpcId.WorkshopBossAbuSamir when relationship.Trust >= 15 => "abu_samir_warm",
            NpcId.WorkshopBossAbuSamir => "abu_samir",
            NpcId.CafeOwnerNadia when maintainingDoubleLife => "nadia_cafe_double_life",
            NpcId.CafeOwnerNadia when relationship.Trust >= 15 => "nadia_cafe_warm",
            NpcId.CafeOwnerNadia => "nadia_cafe",
            NpcId.FenceHanan when relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "hanan_fence_warm",
            NpcId.FenceHanan => "hanan_fence",
            NpcId.RunnerYoussef when policePressure >= 70 => "youssef_runner_hot",
            NpcId.RunnerYoussef => "youssef_runner",
            NpcId.PharmacistMariam when relationship.Trust >= 15 => "mariam_pharmacy_warm",
            NpcId.PharmacistMariam => "mariam_pharmacy",
            NpcId.DispatcherSafaa when relationship.Trust >= 15 => "safaa_depot_warm",
            NpcId.DispatcherSafaa => "safaa_depot",
            NpcId.LaundryOwnerIman when relationship.Trust >= 15 => "iman_laundry_warm",
            NpcId.LaundryOwnerIman => "iman_laundry",
            _ => throw new ArgumentOutOfRangeException(nameof(npcId))
        };
    }
}