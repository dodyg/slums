using System.Linq;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Information;

public sealed class TipState
{
    private readonly List<Tip> _tips = [];
    private readonly Dictionary<NpcId, int> _ignoredByNpc = [];

    public IReadOnlyList<Tip> AllTips => _tips;

    public void AddTip(Tip tip)
    {
        ArgumentNullException.ThrowIfNull(tip);
        _tips.Add(tip);
    }

    public IReadOnlyList<Tip> GetActiveTips(int currentDay)
    {
        return _tips
            .Where(t => !t.IsExpired(currentDay))
            .OrderByDescending(t => t.DayGenerated)
            .ToArray();
    }

    public IReadOnlyList<Tip> GetUndeliveredTips(int currentDay)
    {
        return _tips
            .Where(t => !t.Delivered && !t.IsExpired(currentDay))
            .ToArray();
    }

    public IReadOnlyList<Tip> GetTipsByType(TipType type)
    {
        return _tips.Where(t => t.Type == type).ToArray();
    }

    public IReadOnlyList<Tip> GetTipsFromNpc(NpcId npc)
    {
        return _tips.Where(t => t.Source == npc).ToArray();
    }

    public Tip? GetTip(string id)
    {
        return _tips.FirstOrDefault(t => t.Id == id);
    }

    public bool AcknowledgeTip(string id)
    {
        var index = _tips.FindIndex(t => t.Id == id);
        if (index < 0)
        {
            return false;
        }

        var tip = _tips[index];
        if (tip.Acknowledged || tip.Ignored)
        {
            return false;
        }

        _tips[index] = tip.WithAcknowledged();
        return true;
    }

    public int IgnoreTip(string id)
    {
        var index = _tips.FindIndex(t => t.Id == id);
        if (index < 0)
        {
            return 0;
        }

        var tip = _tips[index];
        if (tip.Acknowledged || tip.Ignored)
        {
            return 0;
        }

        _tips[index] = tip.WithIgnored();

        var npc = tip.Source;
        _ignoredByNpc[npc] = _ignoredByNpc.TryGetValue(npc, out var count) ? count + 1 : 1;

        return _ignoredByNpc[npc];
    }

    public int GetIgnoredCount(NpcId npc)
    {
        return _ignoredByNpc.TryGetValue(npc, out var count) ? count : 0;
    }

    public int RemoveExpired(int currentDay)
    {
        var removed = _tips.Count(t => t.IsExpired(currentDay));
        _tips.RemoveAll(t => t.IsExpired(currentDay));
        return removed;
    }

    public void MarkAsDelivered(string id)
    {
        var index = _tips.FindIndex(t => t.Id == id);
        if (index >= 0 && !_tips[index].Delivered)
        {
            _tips[index] = _tips[index].WithDelivered();
        }
    }

    public IReadOnlyDictionary<NpcId, int> IgnoredCounts => _ignoredByNpc;

    public void RestoreTips(IEnumerable<Tip> tips, Dictionary<NpcId, int> ignoredCounts)
    {
        ArgumentNullException.ThrowIfNull(tips);
        ArgumentNullException.ThrowIfNull(ignoredCounts);
        _tips.Clear();
        _tips.AddRange(tips);
        _ignoredByNpc.Clear();
        foreach (var kvp in ignoredCounts)
        {
            _ignoredByNpc[kvp.Key] = kvp.Value;
        }
    }
}
