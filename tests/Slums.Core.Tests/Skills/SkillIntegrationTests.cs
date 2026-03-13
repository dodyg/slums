using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Skills;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Skills;

internal sealed class SkillIntegrationTests
{
    [Test]
    public void ApplyBackground_ShouldGrantExpectedSkillBonuses()
    {
        var player = new PlayerCharacter();

        player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);

        player.Skills.GetLevel(SkillId.Medical).Should().Be(3);
        player.Skills.GetLevel(SkillId.Persuasion).Should().Be(0);
    }

    [Test]
    public void BuyMedicine_ShouldUseReducedCost_WhenMedicalSkillIsHighEnough()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);

        state.GetMedicineCost().Should().Be(42);
    }

    [Test]
    public void SkillProgression_ShouldCapAtTen()
    {
        var skills = new SkillState();

        for (var i = 0; i < 15; i++)
        {
            skills.TryIncrease(SkillId.StreetSmarts, 1, out _);
        }

        skills.GetLevel(SkillId.StreetSmarts).Should().Be(10);
    }
}