using Slums.Core.Relationships;

namespace Slums.Core.Rumors;

public static class RumorPropagator
{
    public static void Propagate(RumorState rumorState, RelationshipState relationships, int currentDay)
    {
        ArgumentNullException.ThrowIfNull(rumorState);
        ArgumentNullException.ThrowIfNull(relationships);

        foreach (var rumor in rumorState.ActiveRumors)
        {
            if (rumor.NpcsWhoHeard.Count >= rumor.AffectedNpcs.Count)
            {
                continue;
            }

            var newlyHeard = new List<NpcId>();
            foreach (var npcId in rumor.AffectedNpcs)
            {
                if (rumor.NpcsWhoHeard.Contains(npcId))
                {
                    continue;
                }

                var relationship = relationships.GetNpcRelationship(npcId);
                bool shouldHear;

                if (rumor.Age == 0)
                {
                    shouldHear = true;
                }
                else if (rumor.Age == 1)
                {
                    shouldHear = rumor.NpcsWhoHeard.Any(heardNpc =>
                    {
                        var heardRelationship = relationships.GetNpcRelationship(heardNpc);
                        return heardRelationship.Trust > 20;
                    });
                }
                else
                {
                    shouldHear = relationship.Trust > 10;
                }

                if (shouldHear)
                {
                    newlyHeard.Add(npcId);
                }
            }

            foreach (var npcId in newlyHeard)
            {
                rumor.NpcsWhoHeard.Add(npcId);
                var trustChange = rumor.TrustModifier;

                var npcRelationship = relationships.GetNpcRelationship(npcId);
                if (!rumor.IsPositive)
                {
                    if (npcRelationship.Trust > 30)
                    {
                        trustChange = trustChange / 2;
                    }
                    else if (npcRelationship.Trust < -10)
                    {
                        trustChange = (int)(trustChange * 1.5);
                    }
                }

                relationships.ModifyNpcTrust(npcId, trustChange);
            }
        }
    }
}
