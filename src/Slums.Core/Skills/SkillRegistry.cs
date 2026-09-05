namespace Slums.Core.Skills;

/// <summary>Central catalog for skill labels and bounded progression benefits.</summary>
public static class SkillRegistry
{
    private static readonly IReadOnlyList<SkillDefinition> Definitions =
    [
        new(SkillId.Medical, "Medical", "Treat illness and navigate clinic care for your mother.",
            new Dictionary<int, string> { [2] = "Improves clinic access.", [4] = "Reduces medicine friction." }),
        new(SkillId.Persuasion, "Persuasion", "Build one-to-one trust and negotiate under pressure.",
            new Dictionary<int, string> { [2] = "Unlocks more work variants.", [3] = "Strengthens trust gains." }),
        new(SkillId.StreetSmarts, "Street Smarts", "Read risk, routes, and the informal economy.",
            new Dictionary<int, string> { [3] = "Lowers crime detection." }),
        new(SkillId.Physical, "Physical", "Carry difficult work through Cairo's heat and bad equipment.",
            new Dictionary<int, string> { [2] = "Unlocks physical work variants.", [3] = "Reduces some work energy costs." }),
        new(SkillId.RobotRepair, "Technical Repair", "Repair locally maintained machines, handsets, and neighborhood infrastructure.",
            new Dictionary<int, string> { [2] = "Assisted bench repairs and repair work.", [4] = "Unlocks handset and battery repair.", [6] = "Improves service recovery.", [8] = "Unlocks paid technical jobs." }),
        new(SkillId.CyberHacking, "Digital Literacy", "Use fallible digital services without confusing access with power.",
            new Dictionary<int, string> { [2] = "Reduces wallet friction.", [4] = "Unlocks digital work variants.", [6] = "Unlocks biometric appeal.", [8] = "Improves selected service information." }),
        new(SkillId.Provisioning, "Provisioning", "Stretch food, water, and household supplies without making scarcity disappear.",
            new Dictionary<int, string> { [2] = "Makes basic meals more efficient.", [4] = "Household herbs improve meals.", [6] = "Preserves one stored meal from a shock.", [8] = "Improves mother-care meals." }),
        new(SkillId.CommunityOrganizing, "Community Organizing", "Coordinate shared resources, attendance, and neighborhood adaptation.",
            new Dictionary<int, string> { [2] = "Softens one skipped-event penalty.", [4] = "Unlocks shared-resource actions.", [6] = "Improves outage recovery.", [8] = "Coordinates a response to local pressure." }),
        new(SkillId.Composure, "Composure", "Keep functioning when debt, work, heat, and authority apply pressure.",
            new Dictionary<int, string> { [2] = "Reduces stress-related work mistakes.", [4] = "Unlocks calm responses.", [6] = "Softens crisis spikes.", [8] = "Preserves a little energy after pressure." })
    ];

    public static IReadOnlyList<SkillDefinition> All => Definitions;

    public static SkillDefinition Get(SkillId skillId)
    {
        return Definitions.First(definition => definition.Id == skillId);
    }
}
