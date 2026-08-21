namespace Slums.Core.World;

public sealed class InfrastructureState
{
    private readonly Dictionary<(DistrictId District, InfrastructureServiceType Service), InfrastructureServiceState> _services = [];

    public IReadOnlyCollection<InfrastructureServiceState> Services => _services.Values;

    public InfrastructureServiceState Get(DistrictId district, InfrastructureServiceType service)
    {
        return _services.GetValueOrDefault((district, service)) ?? new InfrastructureServiceState
        {
            District = district,
            Service = service,
            Severity = InfrastructureSeverity.Normal
        };
    }

    public void StartDisruption(
        DistrictId district,
        InfrastructureServiceType service,
        InfrastructureSeverity severity,
        int durationDays,
        int currentDay,
        string? sourceId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(durationDays, 1);
        if (severity == InfrastructureSeverity.Normal)
        {
            return;
        }

        var key = (district, service);
        var existing = Get(district, service);
        var next = existing.Severity >= severity && existing.RemainingDays >= durationDays
            ? existing
            : new InfrastructureServiceState
            {
                District = district,
                Service = service,
                Severity = (InfrastructureSeverity)Math.Max((int)existing.Severity, (int)severity),
                StartDay = existing.IsActive ? existing.StartDay : currentDay,
                RemainingDays = Math.Max(existing.RemainingDays, durationDays),
                SourceId = sourceId ?? existing.SourceId
            };
        _services[key] = next;
    }

    public void AdvanceDay()
    {
        foreach (var pair in _services.ToArray())
        {
            var state = pair.Value;
            if (state.RemainingDays <= 1)
            {
                _services.Remove(pair.Key);
                continue;
            }

            _services[pair.Key] = state with { RemainingDays = state.RemainingDays - 1 };
        }
    }

    public void Restore(IEnumerable<InfrastructureServiceState> services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services.Clear();
        foreach (var state in services)
        {
            if (state.Severity != InfrastructureSeverity.Normal && state.RemainingDays > 0)
            {
                _services[(state.District, state.Service)] = state;
            }
        }
    }
}
