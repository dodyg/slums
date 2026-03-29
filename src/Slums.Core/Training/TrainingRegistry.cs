using Slums.Core.Relationships;
using Slums.Core.Skills;

namespace Slums.Core.Training;

public sealed class TrainingRegistry
{
    private static readonly TrainingActivity[] Activities =
    [
        new TrainingActivity(
            TrainingActivityType.StudyMedical,
            SkillId.Medical,
            "Study Medical",
            "Review old textbooks and notes with Nurse Salma's guidance.",
            MoneyCost: 0,
            TimeCostMinutes: 150,
            EnergyCost: 18,
            RequiredNpc: NpcId.NurseSalma,
            RequiredTrust: 10,
            RequiresHome: true),
        new TrainingActivity(
            TrainingActivityType.PracticePersuasion,
            SkillId.Persuasion,
            "Practice Persuasion",
            "Hone your words with Umm Karim in the market.",
            MoneyCost: 10,
            TimeCostMinutes: 120,
            EnergyCost: 15,
            RequiredNpc: NpcId.FixerUmmKarim,
            RequiredTrust: 5,
            RequiresHome: false),
        new TrainingActivity(
            TrainingActivityType.StreetDice,
            SkillId.StreetSmarts,
            "Street Dice",
            "Learn the angles and the reads with Youssef.",
            MoneyCost: 15,
            TimeCostMinutes: 120,
            EnergyCost: 12,
            RequiredNpc: NpcId.RunnerYoussef,
            RequiredTrust: 5,
            RequiresHome: false),
        new TrainingActivity(
            TrainingActivityType.RooftopExercise,
            SkillId.Physical,
            "Rooftop Exercise",
            "Run the rooftop circuits alone, push-ups and squats in the evening air.",
            MoneyCost: 0,
            TimeCostMinutes: 180,
            EnergyCost: 20,
            RequiredNpc: null,
            RequiredTrust: 0,
            RequiresHome: true)
    ];

    public static IReadOnlyList<TrainingActivity> AllActivities => Activities;
}
