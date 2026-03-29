using Slums.Core.Relationships;
using Slums.Core.Skills;

namespace Slums.Core.Training;

public sealed record TrainingActivity(
    TrainingActivityType Type,
    SkillId Skill,
    string Name,
    string Description,
    int MoneyCost,
    int TimeCostMinutes,
    int EnergyCost,
    NpcId? RequiredNpc,
    int RequiredTrust,
    bool RequiresHome);
