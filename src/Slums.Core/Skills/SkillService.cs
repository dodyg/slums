using Slums.Core.State;

namespace Slums.Core.Skills;

public static class SkillService
{
    public static bool ApplySkillGain(SkillId skillId, GameSession gameState, out int newLevel)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return gameState.Player.Skills.TryIncrease(skillId, 1, out newLevel);
    }
}