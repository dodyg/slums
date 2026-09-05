using Slums.Core.Diagnostics;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Training;

/// <summary>Applies training availability, costs, progression, and daily limits.</summary>
internal static class TrainingService
{
    internal static IReadOnlyList<TrainingActivity> GetAvailable(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var results = new List<TrainingActivity>();
        foreach (var activity in TrainingRegistry.AllActivities)
        {
            if (activity.RequiresHome && session.World.CurrentLocationId != LocationId.Home)
            {
                continue;
            }

            if (activity.RequiredNpc is NpcId npcId)
            {
                var relationship = session.Relationships.GetNpcRelationship(npcId);
                if (relationship.Trust < activity.RequiredTrust)
                {
                    continue;
                }
            }

            if (session.Player.Skills.GetLevel(activity.Skill) >= 10)
            {
                continue;
            }

            if (session.TrainedSkillsToday.ContainsKey(activity.Skill))
            {
                continue;
            }

            results.Add(activity);
        }

        return results;
    }

    internal static bool Perform(GameSession session, TrainingActivity activity)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(activity);
        var before = session.CaptureStats();

        if (!GetAvailable(session).Contains(activity))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, session.CaptureStats(), $"{activity.Name} not available");
            session.RaiseEvent($"{activity.Name} is not available right now.");
            return false;
        }

        if (session.Player.Stats.Energy < activity.EnergyCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, session.CaptureStats(), $"Too tired (need {activity.EnergyCost} energy, have {session.Player.Stats.Energy})");
            session.RaiseEvent($"You are too tired for {activity.Name}.");
            return false;
        }

        if (session.Player.Stats.Money < activity.MoneyCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, session.CaptureStats(), $"Cannot afford {activity.Name} (cost {activity.MoneyCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"You cannot afford {activity.Name} right now.");
            return false;
        }

        if (session.Clock.Hour < 18 || session.Clock.Hour >= 22)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, session.CaptureStats(), "Not evening hours (18:00-22:00)");
            session.RaiseEvent("You can only train in the evening (18:00-22:00).");
            return false;
        }

        if (session.Player.Skills.GetLevel(activity.Skill) >= 10)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, session.CaptureStats(), $"{activity.Skill} already at max level");
            session.RaiseEvent($"Your {activity.Skill} is already at maximum.");
            return false;
        }

        var actualEnergyCost = activity.EnergyCost;
        var stressModifier = 0;

        if (session.Player.BackgroundType == Characters.BackgroundType.MedicalSchoolDropout && activity.Type == TrainingActivityType.StudyMedical)
        {
            stressModifier = -3;
        }

        if (session.Player.BackgroundType == Characters.BackgroundType.ReleasedPoliticalPrisoner && activity.Type == TrainingActivityType.StreetDice)
        {
            actualEnergyCost = Math.Max(1, actualEnergyCost - 3);
        }

        if (session.Player.BackgroundType == Characters.BackgroundType.SudaneseRefugee && activity.Type == TrainingActivityType.RooftopExercise)
        {
            actualEnergyCost = Math.Max(1, actualEnergyCost - 3);
        }

        if (activity.MoneyCost > 0)
        {
            session.Player.Stats.ModifyMoney(-activity.MoneyCost);
        }

        session.AdvanceTime(activity.TimeCostMinutes);
        session.Player.Stats.ModifyEnergy(-actualEnergyCost);

        var oldLevel = session.Player.Skills.GetLevel(activity.Skill);
        session.ApplySkillGain(activity.Skill);
        var newLevel = session.Player.Skills.GetLevel(activity.Skill);

        session.TrainedSkillsTodayMutable[activity.Skill] = true;

        if (stressModifier != 0)
        {
            session.Player.Stats.ModifyStress(stressModifier);
        }

        session.RaiseEvent(GetFlavorMessage(activity));
        session.RecordMutation(MutationCategories.Training, "TryPerformTraining", before, session.CaptureStats(), $"{activity.Name} ({activity.Skill} {oldLevel}->{newLevel})");
        return true;
    }

    internal static void ClearDaily(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.TrainedSkillsTodayMutable.Clear();
    }

    internal static void Restore(GameSession session, Dictionary<SkillId, bool> trainedSkillsToday)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trainedSkillsToday);
        session.TrainedSkillsTodayMutable.Clear();
        foreach (var pair in trainedSkillsToday)
        {
            session.TrainedSkillsTodayMutable[pair.Key] = pair.Value;
        }
    }

    private static string GetFlavorMessage(TrainingActivity activity)
    {
        return activity.Type switch
        {
            TrainingActivityType.StudyMedical => "The old textbooks feel less foreign now. Knowledge settles in.",
            TrainingActivityType.PracticePersuasion => "Words sharpen. Umm Karim nods approvingly.",
            TrainingActivityType.StreetDice => "The dice talk to you differently after Youssef's lessons.",
            TrainingActivityType.RooftopExercise => "Your muscles burn, but the evening breeze makes it bearable.",
            TrainingActivityType.RobotRepairBench => "A dead drone opens under your hands. The board was never the problem; the seal was.",
            TrainingActivityType.NetworkErrandPractice => "Umm Karim taps the cracked handset twice. \"The wallet asks who you are. Learn to answer better.\"",
            TrainingActivityType.CommunityKitchenPractice => "You portion the lentils twice, then once more. Mona says, \"Baraka is planning, mish magic.\"",
            TrainingActivityType.NeighborhoodMutualAid => "You leave the notebook open beside the water schedule. A shared plan is still work, but it is work no one carries alone.",
            TrainingActivityType.QuietBreathing => "The room does not become calm. You become a little harder to knock out of your own rhythm.",
            _ => $"You practiced {activity.Name}."
        };
    }
}
