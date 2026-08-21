namespace Slums.Core.World.News;

public sealed class NewsState
{
    private readonly List<ActiveNewsFlash> _active = [];
    private readonly List<string> _seenDefinitionIds = [];
    private readonly Dictionary<NewsCategory, int> _lastGeneratedByCategory = [];

    public IReadOnlyList<ActiveNewsFlash> ActiveFlashes => _active;
    public IReadOnlyList<string> SeenDefinitionIds => _seenDefinitionIds;
    public IReadOnlyDictionary<NewsCategory, int> LastGeneratedByCategory => _lastGeneratedByCategory;
    public int LastGeneratedDay { get; private set; }

    public void BeginDay(int currentDay)
    {
        _active.RemoveAll(flash => !flash.IsActive(currentDay));
    }

    public bool HasSeen(string definitionId) => _seenDefinitionIds.Contains(definitionId, StringComparer.Ordinal);

    public int LastGeneratedDayFor(NewsCategory category) => _lastGeneratedByCategory.GetValueOrDefault(category);

    public void Activate(NewsFlashDefinition definition, int currentDay)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _active.RemoveAll(flash => flash.DefinitionId == definition.Id);
        _active.Add(new ActiveNewsFlash
        {
            DefinitionId = definition.Id,
            StartDay = currentDay,
            ExpiryDay = currentDay + definition.DurationDays - 1
        });

        if (!_seenDefinitionIds.Contains(definition.Id, StringComparer.Ordinal))
        {
            _seenDefinitionIds.Add(definition.Id);
        }

        _lastGeneratedByCategory[definition.Category] = currentDay;
        LastGeneratedDay = currentDay;
    }

    public bool TryGetActive(string definitionId, out ActiveNewsFlash flash)
    {
        flash = _active.FirstOrDefault(candidate => candidate.DefinitionId == definitionId)!;
        return flash is not null;
    }

    public bool TryUseResponse(string definitionId, string responseId)
    {
        var index = _active.FindIndex(flash => flash.DefinitionId == definitionId);
        if (index < 0 || _active[index].UsedResponseId is not null)
        {
            return false;
        }

        _active[index] = _active[index] with { UsedResponseId = responseId };
        return true;
    }

    public void Acknowledge(string definitionId)
    {
        var index = _active.FindIndex(flash => flash.DefinitionId == definitionId);
        if (index >= 0)
        {
            _active[index] = _active[index] with { Acknowledged = true };
        }
    }

    public void Restore(
        IEnumerable<ActiveNewsFlash> active,
        IEnumerable<string> seenDefinitionIds,
        IReadOnlyDictionary<NewsCategory, int> lastGeneratedByCategory,
        int lastGeneratedDay)
    {
        ArgumentNullException.ThrowIfNull(active);
        ArgumentNullException.ThrowIfNull(seenDefinitionIds);
        ArgumentNullException.ThrowIfNull(lastGeneratedByCategory);
        _active.Clear();
        _active.AddRange(active);
        _seenDefinitionIds.Clear();
        _seenDefinitionIds.AddRange(seenDefinitionIds.Distinct(StringComparer.Ordinal));
        _lastGeneratedByCategory.Clear();
        foreach (var item in lastGeneratedByCategory)
        {
            _lastGeneratedByCategory[item.Key] = item.Value;
        }

        LastGeneratedDay = lastGeneratedDay;
    }
}
