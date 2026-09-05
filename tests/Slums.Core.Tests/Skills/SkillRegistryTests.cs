using FluentAssertions;
using Slums.Core.Skills;
using TUnit.Core;

namespace Slums.Core.Tests.Skills;

internal sealed class SkillRegistryTests
{
    [Test]
    public void All_ShouldExposeNineStableSkillDefinitions()
    {
        var definitions = SkillRegistry.All;

        definitions.Should().HaveCount(9);
        definitions.Select(static definition => definition.Id).Should().BeEquivalentTo(Enum.GetValues<SkillId>());
        definitions.Select(static definition => definition.DisplayName).Should().BeEquivalentTo(
        [
            "Medical",
            "Persuasion",
            "Street Smarts",
            "Physical",
            "Technical Repair",
            "Digital Literacy",
            "Provisioning",
            "Community Organizing",
            "Composure"
        ]);
    }

    [Test]
    public void TechnologySkillDefinitions_ShouldRetainPersistedIdentifiers()
    {
        SkillRegistry.Get(SkillId.RobotRepair).DisplayName.Should().Be("Technical Repair");
        SkillRegistry.Get(SkillId.CyberHacking).DisplayName.Should().Be("Digital Literacy");
    }

    [Test]
    public void SkillThresholds_ShouldBeSharedAndBounded()
    {
        SkillThresholds.FirstMeaningfulLevel.Should().Be(2);
        SkillThresholds.AdvancedLevel.Should().Be(4);
        SkillThresholds.HighLevel.Should().Be(6);
        SkillThresholds.MasteryLevel.Should().Be(8);
        SkillThresholds.MaximumLevel.Should().Be(10);
    }
}
