using FluentAssertions;
using Slums.Application.Characters;
using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit;
using Slums.TestSupport;

namespace Slums.Application.Tests.Characters;

internal sealed class CharacterSelectionCommandTests
{
    [Test]
    public void SelectGender_AppliesGenderAndRelationshipModifiers()
    {
        var command = new SelectGenderCommand();
        var session = TestSessions.Create();

        command.Execute(session, Gender.Female);

        session.Player.Gender.Should().Be(Gender.Female);
        session.Player.Name.Should().Be(GenderModifiers.DefaultName(Gender.Female));
        session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust.Should().Be(5,
            "female characters start with higher trust from Neighbor Mona");
    }

    [Test]
    public void SelectBackground_AppliesStartingConditions()
    {
        var command = new SelectBackgroundCommand();
        var session = TestSessions.Create();
        var background = TestContent.Catalog.GetBackground(BackgroundType.SudaneseRefugee);

        command.Execute(session, background);

        session.Player.BackgroundType.Should().Be(BackgroundType.SudaneseRefugee);
        session.Player.Stats.Money.Should().Be(background.StartingMoney);
        session.Player.HasSelectedBackground.Should().BeTrue();
    }

    [Test]
    public void SelectBackground_Throws_WhenSessionIsNull()
    {
        var command = new SelectBackgroundCommand();
        var background = TestContent.Catalog.GetBackground(BackgroundType.SudaneseRefugee);

        var act = () => command.Execute(null!, background);

        act.Should().Throw<ArgumentNullException>();
    }
}
