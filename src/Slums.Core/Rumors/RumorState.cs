using Slums.Core.Relationships;

namespace Slums.Core.Rumors;

public sealed class RumorState
{
    private readonly List<Rumor> _activeRumors = [];

    public IReadOnlyList<Rumor> ActiveRumors => _activeRumors;

    public void AddRumor(Rumor rumor)
    {
        ArgumentNullException.ThrowIfNull(rumor);
        _activeRumors.Add(rumor);
    }

    public void RemoveExpired()
    {
        _activeRumors.RemoveAll(r => r.IsExpired);
    }

    public void DecayAll()
    {
        foreach (var rumor in _activeRumors)
        {
            rumor.Decay();
        }
    }

    public IReadOnlyList<Rumor> GetRumorsAffectingNpc(NpcId npcId)
    {
        return _activeRumors.Where(r => r.AffectedNpcs.Contains(npcId)).ToArray();
    }

    public void Clear()
    {
        _activeRumors.Clear();
    }
}
