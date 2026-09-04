using FluentAssertions;
using Slums.Core.Information;
using TUnit;

namespace Slums.Core.Tests.Information;

internal sealed class ContactErosionRuleTests
{
    [Test]
    [Arguments(9, 2, false)]
    [Arguments(9, 3, false)]
    [Arguments(9, 4, false)]
    [Arguments(10, 2, false)]
    [Arguments(10, 3, true)]
    [Arguments(10, 4, true)]
    [Arguments(11, 2, false)]
    [Arguments(11, 3, true)]
    [Arguments(11, 4, true)]
    public void ShouldErode_ShouldApplyTrustAndIgnoredCountBoundaries(int trust, int ignoredCount, bool expected)
    {
        ContactErosionRule.ShouldErode(trust, ignoredCount).Should().Be(expected);
    }

    [Test]
    public void IgnoreMessage_ShouldErodeOnceForEachNewQualifyingIgnore()
    {
        var session = new Slums.Core.State.GameSession();
        session.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona, 15, 0);

        for (var i = 0; i < 3; i++)
        {
            session.PhoneMessages.AddMessage(new Slums.Core.Phone.PhoneMessage
            {
                Id = $"erosion-{i}",
                Sender = "Mona",
                SenderNpcId = "NeighborMona",
                Content = "A message",
                DayReceived = 1
            });
            session.IgnoreMessage($"erosion-{i}");
        }

        session.Relationships.GetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona).Trust.Should().Be(14);

        session.PhoneMessages.AddMessage(new Slums.Core.Phone.PhoneMessage
        {
            Id = "erosion-3",
            Sender = "Mona",
            SenderNpcId = "NeighborMona",
            Content = "Another message",
            DayReceived = 1
        });
        session.IgnoreMessage("erosion-3");

        session.Relationships.GetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona).Trust.Should().Be(13);
    }
}
