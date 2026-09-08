using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Heat;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Information;

public static class TipGenerator
{
    private static readonly NpcId[] EmployerNpcs =
    [
        NpcId.NurseSalma, NpcId.WorkshopBossAbuSamir,
        NpcId.CafeOwnerNadia, NpcId.PharmacistMariam,
        NpcId.DispatcherSafaa, NpcId.LaundryOwnerIman
    ];

    private static readonly NpcId[] CriminalNpcs =
    [
        NpcId.FenceHanan, NpcId.RunnerYoussef, NpcId.FixerUmmKarim
    ];

    public static IReadOnlyList<Tip> GenerateTips(
        int currentDay,
        RelationshipState relationships,
        DistrictHeatState districtHeat,
        NpcEconomyState npcEconomies,
        BackgroundType background,
        int crimesCommitted,
        int landlordTrust,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(districtHeat);
        ArgumentNullException.ThrowIfNull(npcEconomies);
        ArgumentNullException.ThrowIfNull(random);

        var tips = new List<Tip>();

        TryGeneratePoliceTips(tips, currentDay, relationships, districtHeat, random);
        TryGenerateJobLeads(tips, currentDay, relationships, random);
        TryGenerateMarketIntel(tips, currentDay, relationships, random);
        TryGenerateCrimeWarnings(tips, currentDay, relationships, districtHeat, crimesCommitted, random);
        TryGeneratePersonalWarnings(tips, currentDay, relationships, npcEconomies, landlordTrust, random);
        TryGenerateBackgroundTips(tips, currentDay, relationships, background, districtHeat, random);

        return tips;
    }

    private static void TryGeneratePoliceTips(
        List<Tip> tips, int currentDay, RelationshipState relationships,
        DistrictHeatState districtHeat, Random random)
    {
        var khalidTrust = relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust;
        if (khalidTrust < 20)
        {
            var ummKarimTrust = relationships.GetNpcRelationship(NpcId.FixerUmmKarim).Trust;
            if (ummKarimTrust < 15)
            {
                return;
            }

            if (random.NextDouble() >= 0.20)
            {
                return;
            }

            var highHeat = districtHeat.GetHighHeatDistricts(30);
            if (highHeat.Count == 0)
            {
                return;
            }

            var district = highHeat[random.Next(highHeat.Count)];
            tips.Add(new Tip
            {
                Type = TipType.PoliceTip,
                Source = NpcId.FixerUmmKarim,
                Content = $"Word on the street -- police are planning something in {district}. Be careful.",
                DayGenerated = currentDay,
                ExpiresAfterDay = currentDay + 2,
                RelevantDistrict = district
            });
            return;
        }

        if (random.NextDouble() >= 0.30)
        {
            return;
        }

        var hotDistricts = districtHeat.GetHighHeatDistricts(30);
        if (hotDistricts.Count == 0)
        {
            return;
        }

        var selectedDistrict = hotDistricts[random.Next(hotDistricts.Count)];
        var isEmergency = districtHeat.GetHeat(selectedDistrict) >= PolicePressureThresholds.Hot;

        tips.Add(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = isEmergency
                ? $"Raid coming in {selectedDistrict}. Stay away if you can."
                : $"Heads up -- extra patrols planned for {selectedDistrict} tomorrow.",
            DayGenerated = currentDay,
            ExpiresAfterDay = currentDay + 1,
            RelevantDistrict = selectedDistrict,
            IsEmergency = isEmergency
        });
    }

    private static void TryGenerateJobLeads(
        List<Tip> tips, int currentDay, RelationshipState relationships, Random random)
    {
        if (random.NextDouble() >= 0.25)
        {
            return;
        }

        var eligible = EmployerNpcs
            .Where(npc => relationships.GetNpcRelationship(npc).Trust >= 10)
            .ToArray();

        if (eligible.Length == 0)
        {
            return;
        }

        var npc = eligible[random.Next(eligible.Length)];

        tips.Add(new Tip
        {
            Type = TipType.JobLead,
            Source = npc,
            Content = "Things are picking up. Come by tomorrow if you want extra shifts.",
            DayGenerated = currentDay,
            ExpiresAfterDay = currentDay + 1
        });
    }

    private static void TryGenerateMarketIntel(
        List<Tip> tips, int currentDay, RelationshipState relationships, Random random)
    {
        if (random.NextDouble() >= 0.15)
        {
            return;
        }

        var shopkeeper = NpcId.PharmacistMariam;
        var trust = relationships.GetNpcRelationship(shopkeeper).Trust;
        if (trust < 5)
        {
            return;
        }

        tips.Add(new Tip
        {
            Type = TipType.MarketIntel,
            Source = shopkeeper,
            Content = "Prices are shifting. Stock up on essentials today if you can.",
            DayGenerated = currentDay,
            ExpiresAfterDay = currentDay + 1
        });
    }

    private static void TryGenerateCrimeWarnings(
        List<Tip> tips, int currentDay, RelationshipState relationships,
        DistrictHeatState districtHeat, int crimesCommitted, Random random)
    {
        if (crimesCommitted <= 0)
        {
            return;
        }

        if (random.NextDouble() >= 0.20)
        {
            return;
        }

        var eligible = CriminalNpcs
            .Where(npc => relationships.GetNpcRelationship(npc).Trust >= 10)
            .ToArray();

        if (eligible.Length == 0)
        {
            return;
        }

        var npc = eligible[random.Next(eligible.Length)];

        var highHeat = districtHeat.GetHighHeatDistricts(40);
        DistrictId? district = highHeat.Count > 0 ? highHeat[random.Next(highHeat.Count)] : null;

        var content = district.HasValue
            ? $"Surveillance is up in {district}. Rival crews are watching too."
            : "Things are tight everywhere right now. Keep your head down.";

        tips.Add(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = npc,
            Content = content,
            DayGenerated = currentDay,
            ExpiresAfterDay = currentDay + 1,
            RelevantDistrict = district
        });
    }

    private static void TryGeneratePersonalWarnings(
        List<Tip> tips, int currentDay, RelationshipState relationships,
        NpcEconomyState npcEconomies, int landlordTrust, Random random)
    {
        if (random.NextDouble() >= 0.05)
        {
            return;
        }

        var struggling = npcEconomies.GetStrugglingNpcs();
        if (struggling.Count > 0)
        {
            var npc = struggling[random.Next(struggling.Count)];
            tips.Add(new Tip
            {
                Type = TipType.PersonalWarning,
                Source = NpcId.NeighborMona,
                Content = $"{NpcRegistry.GetName(npc)} is in trouble. Might need help soon.",
                DayGenerated = currentDay,
                ExpiresAfterDay = currentDay + 2
            });
            return;
        }

        if (landlordTrust < 0)
        {
            tips.Add(new Tip
            {
                Type = TipType.PersonalWarning,
                Source = NpcId.NeighborMona,
                Content = "The landlord has been asking about you. Not in a good way.",
                DayGenerated = currentDay,
                ExpiresAfterDay = currentDay + 2
            });
        }
    }

    private static void TryGenerateBackgroundTips(
        List<Tip> tips, int currentDay, RelationshipState relationships,
        BackgroundType background, DistrictHeatState districtHeat, Random random)
    {
        if (random.NextDouble() >= 0.10)
        {
            return;
        }

        switch (background)
        {
            case BackgroundType.SudaneseRefugee:
            {
                var npc = NpcId.FixerUmmKarim;
                if (relationships.GetNpcRelationship(npc).Trust >= 10)
                {
                    tips.Add(new Tip
                    {
                        Type = TipType.PersonalWarning,
                        Source = npc,
                        Content = "Your people should avoid Dokki for a few days. I heard things.",
                        DayGenerated = currentDay,
                        ExpiresAfterDay = currentDay + 2,
                        RelevantDistrict = DistrictId.Dokki
                    });
                }
                break;
            }
            case BackgroundType.ReleasedPoliticalPrisoner:
            {
                var npc = NpcId.RunnerYoussef;
                if (relationships.GetNpcRelationship(npc).Trust >= 10)
                {
                    var highHeat = districtHeat.GetHighHeatDistricts(30);
                    var district = highHeat.Count > 0 ? highHeat[0] : (DistrictId?)null;

                    tips.Add(new Tip
                    {
                        Type = TipType.PoliceTip,
                        Source = npc,
                        Content = district.HasValue
                            ? $"My contacts say they are watching {district} closely. Old network intel."
                            : "The old network says things are calm for now. Stay ready.",
                        DayGenerated = currentDay,
                        ExpiresAfterDay = currentDay + 2,
                        RelevantDistrict = district
                    });
                }
                break;
            }
            case BackgroundType.MedicalSchoolDropout:
            {
                var npc = NpcId.NurseSalma;
                if (relationships.GetNpcRelationship(npc).Trust >= 10)
                {
                    tips.Add(new Tip
                    {
                        Type = TipType.MarketIntel,
                        Source = npc,
                        Content = "A supply shipment is coming to the clinic. I can set aside some basics for you.",
                        DayGenerated = currentDay,
                        ExpiresAfterDay = currentDay + 2
                    });
                }
                break;
            }
        }
    }
}
