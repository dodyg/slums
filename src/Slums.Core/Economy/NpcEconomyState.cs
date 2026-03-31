using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public sealed class NpcEconomyState
{
    private readonly Dictionary<NpcId, NpcEconomy> _economies = [];

    public IReadOnlyDictionary<NpcId, NpcEconomy> Economies => _economies;

    public void Initialize()
    {
        foreach (var def in NpcEconomyDefinitions.All)
        {
            _economies[def.Npc] = new NpcEconomy
            {
                Npc = def.Npc,
                WealthLevel = def.StartingWealth,
                Generosity = def.Generosity
            };
        }
    }

    public NpcEconomy GetEconomy(NpcId npc)
    {
        return _economies.TryGetValue(npc, out var economy)
            ? economy
            : new NpcEconomy { Npc = npc, WealthLevel = NpcWealthLevel.Stable, Generosity = 5 };
    }

    public void SetEconomy(NpcId npc, NpcEconomy economy)
    {
        _economies[npc] = economy;
    }

    public void SetWealthLevel(NpcId npc, NpcWealthLevel level)
    {
        if (!_economies.TryGetValue(npc, out var economy))
        {
            return;
        }

        _economies[npc] = economy with { WealthLevel = level };
    }

    public void AddDebt(DebtorId from, DebtorId to, int amount)
    {
        if (from is DebtorId.NpcDebtor npcFrom)
        {
            var fromEcon = GetEconomy(npcFrom.Npc);
            var owedTo = new Dictionary<DebtorId, int>(fromEcon.MoneyOwedTo);
            owedTo[to] = owedTo.TryGetValue(to, out var existing) ? existing + amount : amount;
            _economies[npcFrom.Npc] = fromEcon with { MoneyOwedTo = owedTo };
        }

        if (to is DebtorId.NpcDebtor npcTo)
        {
            var toEcon = GetEconomy(npcTo.Npc);
            var owedBy = new Dictionary<DebtorId, int>(toEcon.MoneyOwedBy);
            owedBy[from] = owedBy.TryGetValue(from, out var existing) ? existing + amount : amount;
            _economies[npcTo.Npc] = toEcon with { MoneyOwedBy = owedBy };
        }
    }

    public void ResolveDebt(DebtorId from, DebtorId to)
    {
        if (from is DebtorId.NpcDebtor npcFrom)
        {
            var fromEcon = GetEconomy(npcFrom.Npc);
            var owedTo = new Dictionary<DebtorId, int>(fromEcon.MoneyOwedTo);
            owedTo.Remove(to);
            _economies[npcFrom.Npc] = fromEcon with { MoneyOwedTo = owedTo };
        }

        if (to is DebtorId.NpcDebtor npcTo)
        {
            var toEcon = GetEconomy(npcTo.Npc);
            var owedBy = new Dictionary<DebtorId, int>(toEcon.MoneyOwedBy);
            owedBy.Remove(from);
            _economies[npcTo.Npc] = toEcon with { MoneyOwedBy = owedBy };
        }
    }

    public IReadOnlyList<NpcId> GetStrugglingNpcs()
    {
        return _economies
            .Where(static kvp => kvp.Value.WealthLevel == NpcWealthLevel.Struggling)
            .Select(static kvp => kvp.Key)
            .ToArray();
    }

    public IReadOnlyList<NpcId> GetComfortableNpcs()
    {
        return _economies
            .Where(static kvp => kvp.Value.WealthLevel == NpcWealthLevel.Comfortable)
            .Select(static kvp => kvp.Key)
            .ToArray();
    }

    public void RestoreEntry(NpcId npc, NpcWealthLevel wealthLevel, int generosity,
        Dictionary<DebtorId, int> owedTo, Dictionary<DebtorId, int> owedBy,
        int lastHardshipDay, int lastWindfallDay, int generousUntilDay)
    {
        _economies[npc] = new NpcEconomy
        {
            Npc = npc,
            WealthLevel = wealthLevel,
            Generosity = generosity,
            MoneyOwedTo = owedTo,
            MoneyOwedBy = owedBy,
            LastHardshipDay = lastHardshipDay,
            LastWindfallDay = lastWindfallDay,
            GenerousUntilDay = generousUntilDay
        };
    }
}
