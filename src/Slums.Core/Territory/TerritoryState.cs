using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Territory;

public sealed class TerritoryState
{
    private readonly Dictionary<DistrictId, TerritoryControl> _districts = [];
    private bool _initialized;

    public TerritoryControl GetControl(DistrictId district)
    {
        return _districts.TryGetValue(district, out var control)
            ? control
            : new TerritoryControl(district, [], 0, 0);
    }

    public void SetInfluence(DistrictId district, FactionId faction, int value)
    {
        if (!_districts.TryGetValue(district, out var control))
        {
            return;
        }

        _districts[district] = control.SetInfluence(faction, value);
    }

    public void ModifyInfluence(DistrictId district, FactionId faction, int delta)
    {
        if (!_districts.TryGetValue(district, out var control))
        {
            return;
        }

        _districts[district] = control.ModifyInfluence(faction, delta);
    }

    public void ModifyTension(DistrictId district, int delta)
    {
        if (!_districts.TryGetValue(district, out var control))
        {
            return;
        }

        _districts[district] = control.ModifyTension(delta);
    }

    public void Initialize(BackgroundType backgroundType)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        _districts[DistrictId.Imbaba] = CreateEntry(DistrictId.Imbaba, FactionId.ImbabaCrew, 60, FactionId.DokkiThugs, 10, FactionId.ExPrisonerNetwork, 15, 20);
        _districts[DistrictId.Dokki] = CreateEntry(DistrictId.Dokki, FactionId.ImbabaCrew, 5, FactionId.DokkiThugs, 50, FactionId.ExPrisonerNetwork, 10, 30);
        _districts[DistrictId.ArdAlLiwa] = CreateEntry(DistrictId.ArdAlLiwa, FactionId.ImbabaCrew, 20, FactionId.DokkiThugs, 20, FactionId.ExPrisonerNetwork, 40, 45);
        _districts[DistrictId.BulaqAlDakrour] = CreateEntry(DistrictId.BulaqAlDakrour, FactionId.ImbabaCrew, 15, FactionId.DokkiThugs, 15, FactionId.ExPrisonerNetwork, 30, 15);
        _districts[DistrictId.Shubra] = CreateEntry(DistrictId.Shubra, FactionId.ImbabaCrew, 10, FactionId.DokkiThugs, 10, FactionId.ExPrisonerNetwork, 20, 10);
        _districts[DistrictId.DowntownCairo] = CreateEntry(DistrictId.DowntownCairo, FactionId.ImbabaCrew, 5, FactionId.DokkiThugs, 5, FactionId.ExPrisonerNetwork, 10, 5);

        if (backgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            foreach (DistrictId d in Enum.GetValues<DistrictId>())
            {
                ModifyInfluence(d, FactionId.ExPrisonerNetwork, 10);
            }
        }
    }

    private static TerritoryControl CreateEntry(
        DistrictId district,
        FactionId f1, int v1,
        FactionId f2, int v2,
        FactionId f3, int v3,
        int tension)
    {
        var influence = new Dictionary<FactionId, int> { [f1] = v1, [f2] = v2, [f3] = v3 };
        return new TerritoryControl(district, influence, tension, 0);
    }

    public void RestoreEntry(DistrictId district, Dictionary<FactionId, int> influence, int tension, int lastConflictDay)
    {
        _districts[district] = new TerritoryControl(district, influence, tension, lastConflictDay);
    }

    public IReadOnlyDictionary<DistrictId, TerritoryControl> Districts => _districts;

    public bool IsInitialized => _initialized;
}
