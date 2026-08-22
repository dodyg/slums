using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TalkNpcCommandTests
{
    [Test]
    public void Execute_ShouldAdvanceTimeAndRecordOneConversation()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        session.Clock.SetTime(1, 8, 0);
        var command = new TalkNpcCommand();
        var before = (session.Clock.Hour * 60) + session.Clock.Minute;

        var request = command.Execute(session, NpcId.LandlordHajjMahmoud, new Random(1));

        request.Should().NotBeNull();
        var after = (session.Clock.Hour * 60) + session.Clock.Minute;
        after.Should().Be(before + GameSession.ConversationDurationMinutes);
        session.Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud).LastSeenDay.Should().Be(1);
    }

    [Test]
    public void Execute_ShouldRejectASecondMeaningfulConversationWithTheSameNpcThatDay()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        var command = new TalkNpcCommand();
        command.Execute(session, NpcId.LandlordHajjMahmoud, new Random(1));
        var before = (session.Clock.Day * 1440) + (session.Clock.Hour * 60) + session.Clock.Minute;

        var request = command.Execute(session, NpcId.LandlordHajjMahmoud, new Random(2));

        request.Should().BeNull();
        var after = (session.Clock.Day * 1440) + (session.Clock.Hour * 60) + session.Clock.Minute;
        after.Should().Be(before);
    }
}
