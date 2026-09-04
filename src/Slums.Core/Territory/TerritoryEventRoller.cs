using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Territory;

internal static class TerritoryEventRoller
{
    internal static void Roll(GameSession session, Random random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            if (!TerritoryDynamicsCalculator.ShouldTriggerConflictEvent(session.Territory, district, random))
            {
                continue;
            }

            var control = session.Territory.GetControl(district);
            if (control.TensionLevel == TensionLevel.Dangerous)
            {
                if (district == session.World.CurrentDistrict)
                {
                    var crossfire = TerritoryEventRegistry.CrossfireEvent;
                    session.Player.Stats.ModifyStress(crossfire.StressModifier);
                    session.Player.Stats.ModifyHealth(crossfire.HealthModifier);
                    session.RaiseEvent(crossfire.Narration!);
                }
                else
                {
                    session.RaiseEvent($"Fighting breaks out in {district}. The streets are dangerous.");
                }
            }
            else
            {
                var argument = TerritoryEventRegistry.StreetArgument;
                if (district == session.World.CurrentDistrict)
                {
                    session.Player.Stats.ModifyStress(argument.StressModifier);
                    session.RaiseEvent(argument.Narration!);
                }
                else
                {
                    session.RaiseEvent($"Tensions flare in {district}. Word spreads through the neighborhood.");
                }
            }

            if (TerritoryDynamicsCalculator.ShouldTriggerPoliceCrackdown(session.Territory, district, session.DistrictHeat.GetHeat(district)))
            {
                var beforeFlip = session.Territory.GetControl(district);
                TerritoryDynamicsCalculator.ApplyPoliceCrackdown(session.Territory, district);
                session.DistrictHeat.AddHeat(district, 10);
                var crackdown = TerritoryEventRegistry.PoliceCrackdownEvent;

                if (district == session.World.CurrentDistrict)
                {
                    session.Player.Stats.ModifyStress(crackdown.StressModifier);
                    session.RaiseEvent(crackdown.Narration!);
                }
                else
                {
                    session.RaiseEvent($"Police crack down hard in {district}. The whole city feels it.");
                }

                var afterCrackdown = session.Territory.GetControl(district);
                var flip = TerritoryDynamicsCalculator.DetectTerritoryFlip(beforeFlip, afterCrackdown);
                if (flip.HasValue)
                {
                    var flipEvent = TerritoryEventRegistry.TerritoryFlipEvent(flip);
                    session.RaiseEvent(flipEvent.Narration!);
                }
            }
        }

        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && session.World.CurrentDistrict == DistrictId.Imbaba)
        {
            var control = session.Territory.GetControl(DistrictId.Imbaba);
            if (control.TensionLevel >= TensionLevel.Elevated)
            {
#pragma warning disable CA5394
                if (random.Next(100) < 15)
#pragma warning restore CA5394
                {
                    var solidarity = TerritoryEventRegistry.RefugeeSolidarityEvent;
                    session.Player.Stats.ModifyStress(solidarity.StressModifier);
                    session.Territory.ModifyTension(DistrictId.Imbaba, solidarity.TensionModifier);
                    session.RaiseEvent(solidarity.Narration!);
                }
            }
        }
    }
}
