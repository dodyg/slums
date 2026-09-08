using Slums.Core.Diagnostics;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Characters;

/// <summary>Applies household asset purchases, care, upgrades, and encounters.</summary>
internal static class HouseholdAssetsService
{
    internal static bool CanUse(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.World.CurrentLocationId == LocationId.FishMarket
            || session.World.CurrentLocationId == LocationId.PlantShop
            || session.World.CurrentLocationId == LocationId.Workshop
            || (session.World.CurrentLocationId == LocationId.Home
                && (session.Player.HouseholdAssets.HasAnyAssets
                    || session.Player.HouseholdAssets.HasStreetCatEncounter
                    || session.Player.Robotics.HasAnyRobots));
    }

    internal static void Restore(
        GameSession session,
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings,
        IEnumerable<OwnedRobot>? robots,
        int robotParts)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(pets);
        ArgumentNullException.ThrowIfNull(plants);
        session.Player.HouseholdAssets.Restore(pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings);
        session.Player.Robotics.Restore(robots ?? [], robotParts);
    }

    internal static void ResolveWeekly(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var resolution = session.Player.HouseholdAssets.ResolveWeeklyNeglect(session.CurrentWeek);
        if (resolution.StressPenalty <= 0)
        {
            return;
        }

        session.Player.Stats.ModifyStress(resolution.StressPenalty);
        session.RaiseAutoTransaction($"Skipping household care all week weighs on your mother. Stress +{resolution.StressPenalty}.");
    }

}
