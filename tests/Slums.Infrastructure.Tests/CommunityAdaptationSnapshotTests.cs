using FluentAssertions;
using Slums.Core.Community;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class CommunityAdaptationSnapshotTests
{
    [Test]
    public void CaptureAndRestore_ShouldPreserveGroupOutcomesAndSkillLevels()
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.CommunityOrganizing, 8);
        session.CommunityAdaptation.AddCoolingRoomDays(3);
        session.CommunityAdaptation.AddWaterReserve(2);
        session.CommunityAdaptation.RecordSuccessfulAction(2);

        var restored = GameSessionSnapshot.Capture(session).Restore();

        restored.Player.Skills.GetLevel(SkillId.CommunityOrganizing).Should().Be(8);
        restored.CommunityAdaptation.CoolingRoomDaysRemaining.Should().Be(3);
        restored.CommunityAdaptation.WaterReserveUnits.Should().Be(2);
        restored.CommunityAdaptation.SuccessfulActions.Should().Be(1);
        restored.CommunityAdaptation.ShelterContributions.Should().Be(2);
    }
}
