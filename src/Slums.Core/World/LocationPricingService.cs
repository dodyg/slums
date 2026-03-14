using Slums.Core.Expenses;
using Slums.Core.Relationships;
using Slums.Core.Skills;

namespace Slums.Core.World;

internal sealed class LocationPricingService
{
#pragma warning disable CA1822
    public int GetClinicVisitCost(Location location, RelationshipState relationships, SkillState skills)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(skills);

        var visitCost = location.ClinicVisitBaseCost;
        if (visitCost <= 0)
        {
            return 0;
        }

        if (skills.GetLevel(SkillId.Medical) >= 2)
        {
            visitCost = Math.Max(20, visitCost - 5);
        }

        if (location.Id == LocationId.Clinic && relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 20)
        {
            visitCost = Math.Max(18, visitCost - 6);
        }

        if (location.Id == LocationId.Pharmacy && relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            visitCost = Math.Max(20, visitCost - 4);
        }

        return visitCost;
    }

    public int GetFoodCost(DistrictId districtId)
    {
        return districtId switch
        {
            DistrictId.Dokki => 20,
            DistrictId.Imbaba => 15,
            DistrictId.ArdAlLiwa => 13,
            DistrictId.BulaqAlDakrour => 14,
            DistrictId.Shubra => 17,
            _ => RecurringExpenses.CheapFoodStockpile
        };
    }

    public int GetMedicineCost(DistrictId districtId, LocationId currentLocationId, RelationshipState relationships, SkillState skills)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(skills);

        var districtCost = districtId switch
        {
            DistrictId.Dokki => 58,
            DistrictId.Imbaba => 50,
            DistrictId.ArdAlLiwa => 42,
            DistrictId.BulaqAlDakrour => 46,
            DistrictId.Shubra => 52,
            _ => RecurringExpenses.MedicineCost
        };

        if (currentLocationId == LocationId.Pharmacy && relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            districtCost = Math.Max(30, districtCost - 6);
        }

        return skills.GetLevel(SkillId.Medical) >= 3
            ? Math.Max(32, districtCost - 8)
            : districtCost;
    }

    public int GetStreetFoodCost(DistrictId districtId)
    {
        return districtId switch
        {
            DistrictId.Dokki => 10,
            DistrictId.Imbaba => 8,
            DistrictId.ArdAlLiwa => 7,
            DistrictId.BulaqAlDakrour => 7,
            DistrictId.Shubra => 9,
            _ => 8
        };
    }

    public int GetTravelCost(Location destination, RelationshipState relationships)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(relationships);

        var travelCost = RecurringExpenses.TravelCost;
        if (destination.District == DistrictId.BulaqAlDakrour && relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            travelCost = Math.Max(1, travelCost - 1);
        }

        return travelCost;
    }

    public int GetTravelEnergyCost(Location destination, RelationshipState relationships)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(relationships);

        var energyCost = 5;
        if (destination.District == DistrictId.BulaqAlDakrour && relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            energyCost = 3;
        }

        if (destination.District == DistrictId.Shubra && relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 12)
        {
            energyCost = Math.Max(2, energyCost - 1);
        }

        return energyCost;
    }
#pragma warning restore CA1822
}
