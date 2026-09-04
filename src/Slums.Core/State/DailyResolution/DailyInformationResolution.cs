using Slums.Core.Relationships;
using Slums.Core.Rumors;

namespace Slums.Core.State.DailyResolution;

/// <summary>
/// Resolves the information-flow blocks of the daily pipeline: community event attendance
/// consequences, rumor propagation, and the daily phone and tip cycles.
/// </summary>
internal static class DailyInformationResolution
{
    internal static void ResolveAttendance(GameSession session)
    {
        if (session.EventAttendance.LastAttendanceDay < session.Clock.Day - 1 || session.Clock.Day == 1)
        {
            session.EventAttendance.RecordSkip();
        }

        session.EventAttendance.ResetWeeklyIfNeeded(session.Clock.Day);

        if (session.EventAttendance.ConsecutiveSkips >= 3)
        {
            var struggling = session.Player.Stats.Money < 30
                || session.Player.Stats.Health < 40
                || session.Player.Stats.Stress > 60;
            if (struggling)
            {
                var concernNpc = NpcId.NeighborMona;
                session.Relationships.ModifyNpcTrust(concernNpc, 1);
                session.RaiseEvent("Mona notices you struggling and drops off some bread. Trust +1.");
            }
        }

        if (session.EventAttendance.ConsecutiveSkips >= 5)
        {
            session.Relationships.ModifyNpcTrust(NpcId.NeighborMona, -1);
            session.Relationships.ModifyNpcTrust(NpcId.FixerUmmKarim, -1);
            session.Relationships.ModifyNpcTrust(NpcId.NurseSalma, -1);
            session.RaiseEvent("Neighbors are starting to talk. You never show up anymore.");
        }
    }

    internal static void ResolveRumors(GameSession session)
    {
        session.Rumors.DecayAll();
        RumorPropagator.Propagate(session.Rumors, session.Relationships, session.Clock.Day);
        foreach (var rumor in session.Rumors.ActiveRumors)
        {
            foreach (var npcId in rumor.AffectedNpcs)
            {
                if (!rumor.NpcsWhoHeard.Contains(npcId))
                {
                    var modifier = rumor.TrustModifier;
                    var relationship = session.Relationships.GetNpcRelationship(npcId);
                    if (relationship.Trust > 30 && !rumor.IsPositive)
                    {
                        modifier = modifier / 2;
                    }
                    else if (relationship.Trust < -10 && !rumor.IsPositive)
                    {
                        modifier = (int)(modifier * 1.5);
                    }

                    if (modifier != 0)
                    {
                        session.Relationships.ModifyNpcTrust(npcId, modifier);
                    }

                    rumor.NpcsWhoHeard.Add(npcId);
                }
            }
        }

        session.Rumors.RemoveExpired();

        if (session.EventAttendance.ConsecutiveSkips >= 3)
        {
            session.Rumors.AddRumor(RumorGenerator.OnSkippingCommunityEvents(session.EventAttendance.ConsecutiveSkips, session.Clock.Day));
        }
    }

    internal static void ResolvePhoneAndTips(GameSession session, Random random)
    {
        session.ProcessDailyPhone(random);
        session.ProcessDailyTips(random);
    }
}
