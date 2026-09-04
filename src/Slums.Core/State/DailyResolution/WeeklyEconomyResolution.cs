using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.World.News;

namespace Slums.Core.State.DailyResolution;

/// <summary>Resolves the weekly NPC economy and its effects on the player.</summary>
internal static class WeeklyEconomyResolution
{
    internal static void Resolve(GameSession session, Random random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);

        var hardshipModifier = NewsImpactCalculator.GetNpcHardshipModifier(session.News);
        NpcEconomyResolver.ResolveWeek(session.NpcEconomies, session.Relationships, session.Clock.Day, random, hardshipModifier);
        if (hardshipModifier > 0)
        {
            session.RaiseEvent($"City pressure is reaching household economies. Local hardship risk is up by {hardshipModifier}.");
        }

        var hajjEconomy = session.NpcEconomies.GetEconomy(NpcId.LandlordHajjMahmoud);
        if (hajjEconomy.WealthLevel is NpcWealthLevel.Struggling or NpcWealthLevel.Poor)
        {
            session.Rent.PayPartialDebt(-10);
            session.RaiseEvent("Hajj Mahmoud's money troubles make him meaner about rent. Rent pressure increases.");
        }

        var monaEconomy = session.NpcEconomies.GetEconomy(NpcId.NeighborMona);
        if (monaEconomy.WealthLevel == NpcWealthLevel.Struggling)
        {
            session.Player.Stats.ModifyStress(3);
            session.RaiseEvent("Mona is struggling. The worry weighs on you.");
        }

        var ummKarimEconomy = session.NpcEconomies.GetEconomy(NpcId.FixerUmmKarim);
        if (ummKarimEconomy.WealthLevel == NpcWealthLevel.Comfortable)
        {
            session.RaiseEvent("Umm Karim is doing well. She slips you an extra portion.");
        }

        var needingLoan = NpcEconomyResolver.GetNpcNeedingLoan(session.NpcEconomies, session.Relationships);
        if (needingLoan.HasValue)
        {
            var npcRel = session.Relationships.GetNpcRelationship(needingLoan.Value);
            if (npcRel.Trust >= 10)
            {
                session.RaiseEvent($"{needingLoan.Value} is in rough shape. They could use help.");
            }
        }

        session.PlayerDebts.ProcessInterest(session.Clock.Day);
        session.PlayerDebts.UpdateCollectionStates(session.Clock.Day);
    }
}
