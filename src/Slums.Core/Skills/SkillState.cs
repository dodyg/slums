namespace Slums.Core.Skills;

public sealed class SkillState
{
    private const int MaxLevel = 10;
    private const int MinLevel = 0;
    private readonly Dictionary<SkillId, int> _levels = Enum
        .GetValues<SkillId>()
        .ToDictionary(static skillId => skillId, static _ => 0);

    public IReadOnlyDictionary<SkillId, int> Levels => _levels;

    public int GetLevel(SkillId skillId)
    {
        return _levels.GetValueOrDefault(skillId);
    }

    public void SetLevel(SkillId skillId, int level)
    {
        _levels[skillId] = Math.Clamp(level, MinLevel, MaxLevel);
    }

    public bool TryIncrease(SkillId skillId, int amount, out int newLevel)
    {
        var previousLevel = GetLevel(skillId);
        var nextLevel = Math.Clamp(previousLevel + amount, MinLevel, MaxLevel);
        _levels[skillId] = nextLevel;
        newLevel = nextLevel;
        return nextLevel > previousLevel;
    }

    public void Restore(IEnumerable<KeyValuePair<SkillId, int>> levels)
    {
        ArgumentNullException.ThrowIfNull(levels);

        foreach (var skillId in Enum.GetValues<SkillId>())
        {
            _levels[skillId] = 0;
        }

        foreach (var pair in levels)
        {
            SetLevel(pair.Key, pair.Value);
        }
    }
}