using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TalkSceneRequestFactoryTests
{
    [Test]
    public void Create_ShouldRecordContactAndConversationHistory()
    {
        using var gameSession = new GameSession();
        gameSession.World.TravelTo(LocationId.Home);

        var context = TalkNpcContext.Create(gameSession);
        var factory = new TalkSceneRequestFactory();

        var request = factory.Create(context, NpcId.LandlordHajjMahmoud);
        var relationship = gameSession.Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud);

        relationship.RecentContactCount.Should().Be(1);
        relationship.LastSeenDay.Should().Be(gameSession.Clock.Day);
        gameSession.Relationships.HasSeenConversation(NpcId.LandlordHajjMahmoud, request.KnotName).Should().BeTrue();
        request.SceneState.Day.Should().Be(gameSession.Clock.Day);
        request.SceneState.Money.Should().Be(gameSession.Player.Stats.Money);
    }
}
