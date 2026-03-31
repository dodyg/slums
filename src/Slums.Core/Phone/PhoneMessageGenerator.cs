using Slums.Core.Characters;
using Slums.Core.Heat;
using Slums.Core.Relationships;

namespace Slums.Core.Phone;

public static class PhoneMessageGenerator
{
    private static readonly NpcId[] CriminalNpcs = [NpcId.FenceHanan, NpcId.RunnerYoussef, NpcId.FixerUmmKarim];

    public static IReadOnlyList<PhoneMessage> GenerateMessages(
        int currentDay,
        RelationshipState relationships,
        int policePressure,
        int motherHealth,
        DistrictHeatState districtHeat,
        BackgroundType background,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(districtHeat);
        ArgumentNullException.ThrowIfNull(random);

        var messages = new List<PhoneMessage>();

        TryGenerateCriminalOpportunity(messages, currentDay, relationships, random);
        TryGenerateHeatWarnings(messages, currentDay, relationships, districtHeat, random);
        TryGenerateFamilyAlert(messages, currentDay, relationships, motherHealth, random);
        TryGenerateNetworkRequests(messages, currentDay, relationships, random);
        TryGenerateBackgroundMessages(messages, currentDay, relationships, background, random);

        return messages;
    }

    private static void TryGenerateCriminalOpportunity(
        List<PhoneMessage> messages, int currentDay, RelationshipState relationships, Random random)
    {
#pragma warning disable CA5394
        if (random.NextDouble() >= 0.20)
        {
            return;
        }
#pragma warning restore CA5394

        var eligible = CriminalNpcs
            .Where(npc => relationships.GetNpcRelationship(npc).Trust >= 10)
            .ToArray();

        if (eligible.Length == 0)
        {
            return;
        }

#pragma warning disable CA5394
        var sender = eligible[random.Next(eligible.Length)];
#pragma warning restore CA5394

        messages.Add(new PhoneMessage
        {
            Type = PhoneMessageType.Opportunity,
            Sender = NpcRegistry.GetName(sender),
            SenderNpcId = sender.ToString(),
            Content = "Meet me at the square tonight. I have something for you.",
            DayReceived = currentDay,
            ExpiresAfterDay = currentDay + 2,
            RequiresResponse = true,
            ResponseTimeCost = 2,
            ResponseMoneyCost = 0
        });
    }

    private static void TryGenerateHeatWarnings(
        List<PhoneMessage> messages, int currentDay, RelationshipState relationships,
        DistrictHeatState districtHeat, Random random)
    {
        foreach (var district in Enum.GetValues<World.DistrictId>())
        {
            var heat = districtHeat.GetHeat(district);
            if (heat <= 60)
            {
                continue;
            }

            var npcForDistrict = district switch
            {
                World.DistrictId.Imbaba => NpcId.LandlordHajjMahmoud,
                World.DistrictId.Dokki => NpcId.WorkshopBossAbuSamir,
                World.DistrictId.BulaqAlDakrour => NpcId.DispatcherSafaa,
                World.DistrictId.Shubra => NpcId.LaundryOwnerIman,
                _ => (NpcId?)null
            };

            if (npcForDistrict is not { } npc)
            {
                continue;
            }

            var trust = relationships.GetNpcRelationship(npc).Trust;
            if (trust < 5)
            {
                continue;
            }

            messages.Add(new PhoneMessage
            {
                Type = PhoneMessageType.Warning,
                Sender = NpcRegistry.GetName(npc),
                SenderNpcId = npc.ToString(),
                Content = $"Don't come to {district} today. Things are heating up.",
                DayReceived = currentDay,
                ExpiresAfterDay = currentDay + 1,
                RequiresResponse = false,
                ResponseTimeCost = 0,
                ResponseMoneyCost = 0
            });
        }
    }

    private static void TryGenerateFamilyAlert(
        List<PhoneMessage> messages, int currentDay, RelationshipState relationships,
        int motherHealth, Random random)
    {
        if (motherHealth >= 40)
        {
            return;
        }

#pragma warning disable CA5394
        if (random.NextDouble() >= 0.30)
        {
            return;
        }
#pragma warning restore CA5394

        var sender = NpcId.NeighborMona;
        var trust = relationships.GetNpcRelationship(sender).Trust;
        if (trust < 0)
        {
            return;
        }

        messages.Add(new PhoneMessage
        {
            Type = PhoneMessageType.FamilyAlert,
            Sender = NpcRegistry.GetName(sender),
            SenderNpcId = sender.ToString(),
            Content = "I heard your mother coughing again last night. You should check on her.",
            DayReceived = currentDay,
            ExpiresAfterDay = currentDay + 3,
            RequiresResponse = true,
            ResponseTimeCost = 1,
            ResponseMoneyCost = 0
        });
    }

    private static void TryGenerateNetworkRequests(
        List<PhoneMessage> messages, int currentDay, RelationshipState relationships, Random random)
    {
#pragma warning disable CA5394
        if (random.NextDouble() >= 0.10)
        {
            return;
        }
#pragma warning restore CA5394

        foreach (var faction in Enum.GetValues<FactionId>())
        {
            var standing = relationships.GetFactionStanding(faction);
            if (standing.Reputation < 5)
            {
                continue;
            }

            var sender = faction switch
            {
                FactionId.ImbabaCrew => NpcId.FixerUmmKarim,
                FactionId.DokkiThugs => NpcId.FenceHanan,
                FactionId.ExPrisonerNetwork => NpcId.RunnerYoussef,
                _ => (NpcId?)null
            };

            if (sender is not { } npc)
            {
                continue;
            }

            messages.Add(new PhoneMessage
            {
                Type = PhoneMessageType.NetworkRequest,
                Sender = NpcRegistry.GetName(npc),
                SenderNpcId = npc.ToString(),
                Content = "I need a small favor. Come find me when you can.",
                DayReceived = currentDay,
                ExpiresAfterDay = currentDay + 3,
                RequiresResponse = true,
                ResponseTimeCost = 2,
                ResponseMoneyCost = 5
            });

            break;
        }
    }

    private static void TryGenerateBackgroundMessages(
        List<PhoneMessage> messages, int currentDay, RelationshipState relationships,
        BackgroundType background, Random random)
    {
#pragma warning disable CA5394
        if (random.NextDouble() >= 0.15)
        {
            return;
        }
#pragma warning restore CA5394

        switch (background)
        {
            case BackgroundType.SudaneseRefugee:
            {
                var npc = NpcId.LandlordHajjMahmoud;
                if (relationships.GetNpcRelationship(npc).Trust >= 5)
                {
                    messages.Add(new PhoneMessage
                    {
                        Type = PhoneMessageType.Background,
                        Sender = NpcRegistry.GetName(npc),
                        SenderNpcId = npc.ToString(),
                        Content = "Some of your people are gathering at the mosque. You should come.",
                        DayReceived = currentDay,
                        ExpiresAfterDay = currentDay + 2,
                        RequiresResponse = true,
                        ResponseTimeCost = 2,
                        ResponseMoneyCost = 0
                    });
                }
                break;
            }
            case BackgroundType.ReleasedPoliticalPrisoner:
            {
                var npc = NpcId.RunnerYoussef;
                if (relationships.GetNpcRelationship(npc).Trust >= 5)
                {
                    messages.Add(new PhoneMessage
                    {
                        Type = PhoneMessageType.Background,
                        Sender = NpcRegistry.GetName(npc),
                        SenderNpcId = npc.ToString(),
                        Content = "Old friend from before is asking about you. Want me to arrange a meeting?",
                        DayReceived = currentDay,
                        ExpiresAfterDay = currentDay + 2,
                        RequiresResponse = true,
                        ResponseTimeCost = 1,
                        ResponseMoneyCost = 0
                    });
                }
                break;
            }
            case BackgroundType.MedicalSchoolDropout:
            {
                var npc = NpcId.NurseSalma;
                if (relationships.GetNpcRelationship(npc).Trust >= 5)
                {
                    messages.Add(new PhoneMessage
                    {
                        Type = PhoneMessageType.Background,
                        Sender = NpcRegistry.GetName(npc),
                        SenderNpcId = npc.ToString(),
                        Content = "I know someone who needs help with medical supplies. Discreet. Interested?",
                        DayReceived = currentDay,
                        ExpiresAfterDay = currentDay + 2,
                        RequiresResponse = true,
                        ResponseTimeCost = 1,
                        ResponseMoneyCost = 0
                    });
                }
                break;
            }
        }
    }
}
