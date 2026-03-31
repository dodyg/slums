using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public static class NpcEconomyResolver
{
    public static void ResolveWeek(NpcEconomyState economies, RelationshipState relationships, int currentDay, Random random)
    {
        ArgumentNullException.ThrowIfNull(economies);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(random);

        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var def = NpcEconomyDefinitions.All.FirstOrDefault(d => d.Npc == npcId);
            if (def is null)
            {
                continue;
            }

            var economy = economies.GetEconomy(npcId);

            if (economy.GenerousUntilDay > 0 && currentDay > economy.GenerousUntilDay)
            {
                economy = economy with { GenerousUntilDay = 0 };
                economies.SetEconomy(npcId, economy);
            }

            int roll;
#pragma warning disable CA5394
            roll = random.Next(100);
#pragma warning restore CA5394

            if (roll < def.HardshipChance)
            {
                ApplyHardship(economies, relationships, npcId, economy, currentDay, random);
            }
            else if (roll < def.HardshipChance + def.WindfallChance)
            {
                int generousDays;
#pragma warning disable CA5394
                generousDays = random.Next(3, 6);
#pragma warning restore CA5394
                economy = economy.WithWindfall(currentDay, currentDay + generousDays);
                economies.SetEconomy(npcId, economy);
            }
        }

        ProcessNpcToNpcDebt(economies, relationships, currentDay);
    }

    private static void ApplyHardship(NpcEconomyState economies, RelationshipState relationships,
        NpcId npcId, NpcEconomy economy, int currentDay, Random random)
    {
        economy = economy.WithHardship(currentDay);
        economies.SetEconomy(npcId, economy);

        var lender = FindBestLender(economies, relationships, npcId);
        if (lender.HasValue)
        {
            int amount;
#pragma warning disable CA5394
            amount = random.Next(20, 41);
#pragma warning restore CA5394
            economies.AddDebt(new DebtorId.NpcDebtor(npcId), new DebtorId.NpcDebtor(lender.Value), amount);
        }
    }

    private static NpcId? FindBestLender(NpcEconomyState economies, RelationshipState relationships, NpcId borrower)
    {
        NpcId? bestLender = null;
        var bestWealth = NpcWealthLevel.Struggling;
        var bestGenerosity = 0;

        foreach (var candidateId in Enum.GetValues<NpcId>())
        {
            if (candidateId == borrower)
            {
                continue;
            }

            var trust = relationships.GetNpcRelationship(candidateId).Trust;
            if (trust <= 10)
            {
                continue;
            }

            var candidateEcon = economies.GetEconomy(candidateId);
            if (candidateEcon.Generosity < 4)
            {
                continue;
            }

            if ((int)candidateEcon.WealthLevel > (int)bestWealth
                || ((int)candidateEcon.WealthLevel == (int)bestWealth && candidateEcon.Generosity > bestGenerosity))
            {
                bestLender = candidateId;
                bestWealth = candidateEcon.WealthLevel;
                bestGenerosity = candidateEcon.Generosity;
            }
        }

        return bestLender;
    }

    private static void ProcessNpcToNpcDebt(NpcEconomyState economies, RelationshipState relationships, int currentDay)
    {
        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var economy = economies.GetEconomy(npcId);
            var debtsCopy = economy.MoneyOwedTo.ToList();

            foreach (var debt in debtsCopy)
            {
                if (debt.Key is not DebtorId.NpcDebtor creditor)
                {
                    continue;
                }

                economies.ResolveDebt(new DebtorId.NpcDebtor(npcId), creditor);

                var creditorEcon = economies.GetEconomy(creditor.Npc);
                if (!creditorEcon.MoneyOwedTo.ContainsKey(new DebtorId.NpcDebtor(npcId)))
                {
                    relationships.ModifyNpcTrust(npcId, -5);
                    relationships.ModifyNpcTrust(creditor.Npc, -5);
                }
            }
        }
    }

    public static NpcId? GetNpcNeedingLoan(NpcEconomyState economies, RelationshipState relationships)
    {
        ArgumentNullException.ThrowIfNull(economies);
        ArgumentNullException.ThrowIfNull(relationships);

        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var economy = economies.GetEconomy(npcId);
            if (economy.WealthLevel != NpcWealthLevel.Struggling)
            {
                continue;
            }

            var totalOwedToOthers = economy.MoneyOwedTo.Values.Sum();
            if (totalOwedToOthers > 0)
            {
                continue;
            }

            var hasNpcLender = false;
            foreach (var candidateId in Enum.GetValues<NpcId>())
            {
                if (candidateId == npcId)
                {
                    continue;
                }

                var trust = relationships.GetNpcRelationship(candidateId).Trust;
                var candidateEcon = economies.GetEconomy(candidateId);
                if (trust > 10 && candidateEcon.Generosity >= 4)
                {
                    hasNpcLender = true;
                    break;
                }
            }

            if (!hasNpcLender)
            {
                return npcId;
            }
        }

        return null;
    }
}
