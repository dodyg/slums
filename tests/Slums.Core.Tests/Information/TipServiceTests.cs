using FluentAssertions;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Information;

internal sealed class TipServiceTests
{
    [Test]
    public void Acknowledge_ShouldUseTheTipServiceAndRecordMutation()
    {
        var session = new GameSession();
        var tip = new Tip
        {
            Id = "service-tip",
            Source = NpcId.NeighborMona,
            Type = TipType.JobLead,
            Content = "A lead",
            DayGenerated = 1,
            ExpiresAfterDay = 5
        };
        session.Tips.AddTip(tip);

        var result = TipService.Acknowledge(session, tip.Id);

        result.Success.Should().BeTrue();
        session.Tips.GetTip(tip.Id)!.Acknowledged.Should().BeTrue();
        session.Mutations[^1].Action.Should().Be("AcknowledgeTip");
    }

    [Test]
    public void Restore_ShouldHydrateTheExistingTipState()
    {
        var session = new GameSession();
        var tips = session.Tips;

        TipService.Restore(
            session,
            [new Tip
            {
                Id = "restored-tip",
                Source = NpcId.NeighborMona,
                Type = TipType.JobLead,
                Content = "Restored",
                DayGenerated = 1,
                ExpiresAfterDay = 5
            }],
            new Dictionary<NpcId, int>());

        session.Tips.Should().BeSameAs(tips);
        session.Tips.AllTips.Should().ContainSingle();
    }
}
