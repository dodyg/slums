using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Territory;

public static class TerritoryDynamicsCalculator
{
    private const int TensionDecayRate = 2;
    private const int CrimeWithApprovalInfluenceGain = 3;
    private const int CrimeWithoutApprovalTensionGain = 5;
    private const int CrimeWithoutApprovalInfluenceLoss = -2;
    private const int FactionCrimeInfluenceGain = 5;
    private const int HonestWorkTensionReduction = -1;
    private const int PoliceCrackdownTensionReduction = -30;
    private const int PoliceCrackdownInfluenceReduction = -10;

    public static void ApplyDailyDecay(TerritoryState territory)
    {
        ArgumentNullException.ThrowIfNull(territory);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            var control = territory.GetControl(district);
            if (control.Tension > 0)
            {
                territory.ModifyTension(district, -TensionDecayRate);
            }
        }
    }

    public static void ApplyCrimeImpact(TerritoryState territory, DistrictId district, FactionId? approvingFaction)
    {
        ArgumentNullException.ThrowIfNull(territory);

        if (approvingFaction.HasValue)
        {
            territory.ModifyInfluence(district, approvingFaction.Value, CrimeWithApprovalInfluenceGain);
        }
        else
        {
            territory.ModifyTension(district, CrimeWithoutApprovalTensionGain);
            foreach (FactionId faction in Enum.GetValues<FactionId>())
            {
                territory.ModifyInfluence(district, faction, CrimeWithoutApprovalInfluenceLoss);
            }
        }
    }

    public static void ApplyFactionCrimeImpact(TerritoryState territory, DistrictId district, FactionId faction)
    {
        ArgumentNullException.ThrowIfNull(territory);

        territory.ModifyInfluence(district, faction, FactionCrimeInfluenceGain);
    }

    public static void ApplyHonestWorkImpact(TerritoryState territory, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(territory);

        territory.ModifyTension(district, HonestWorkTensionReduction);
    }

    public static void ApplyPoliceCrackdown(TerritoryState territory, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(territory);

        territory.ModifyTension(district, PoliceCrackdownTensionReduction);
        foreach (FactionId faction in Enum.GetValues<FactionId>())
        {
            territory.ModifyInfluence(district, faction, PoliceCrackdownInfluenceReduction);
        }
    }

    public static bool ShouldTriggerPoliceCrackdown(TerritoryState territory, DistrictId district, int districtHeat)
    {
        ArgumentNullException.ThrowIfNull(territory);

        var control = territory.GetControl(district);
        return control.Tension > 60 && districtHeat > 40;
    }

    public static int GetFoodPriceModifier(TerritoryState territory, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(territory);

        var control = territory.GetControl(district);
        return control.TensionLevel switch
        {
            TensionLevel.High => 3,
            TensionLevel.Dangerous => 5,
            _ => 0
        };
    }

    public static bool IsCrimeBlocked(TerritoryState territory, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(territory);

        var control = territory.GetControl(district);
        if (control.TensionLevel == TensionLevel.Dangerous)
        {
            return true;
        }

        return false;
    }

    public static bool IsNonControllingCrimeBlocked(TerritoryState territory, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(territory);

        var control = territory.GetControl(district);
        return control.TensionLevel == TensionLevel.High;
    }

    public static bool ShouldTriggerConflictEvent(TerritoryState territory, DistrictId district, Random random)
    {
        ArgumentNullException.ThrowIfNull(territory);
        ArgumentNullException.ThrowIfNull(random);

        var control = territory.GetControl(district);
        var chance = control.TensionLevel switch
        {
            TensionLevel.Elevated => 10,
            TensionLevel.High => 25,
            TensionLevel.Dangerous => 50,
            _ => 0
        };

#pragma warning disable CA5394
        return chance > 0 && random.Next(100) < chance;
#pragma warning restore CA5394
    }

    public static FactionId? DetectTerritoryFlip(TerritoryControl before, TerritoryControl after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);

        return before.ControllingFaction != after.ControllingFaction ? after.ControllingFaction : null;
    }
}
