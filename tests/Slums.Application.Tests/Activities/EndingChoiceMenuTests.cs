using FluentAssertions;
using Slums.Application.Endings;
using Slums.Core.Endings;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class EndingChoiceMenuTests
{
    [Test]
    public void GetOptions_ShouldExposeGoalLabelsAndRequirements()
    {
        var context = new EndingChoiceMenuContext([EndingId.QuitTheLuxorDream, EndingId.NetworkShelter]);

        var options = EndingChoiceMenuQuery.GetOptions(context);

        options.Should().ContainSingle(option => option.Id == EndingId.QuitTheLuxorDream && option.Label == "Leave for Luxor");
        options.Single(option => option.Id == EndingId.QuitTheLuxorDream).Requirements.Should().Contain("550 LE");
        options.Single(option => option.Id == EndingId.NetworkShelter).Label.Should().Be("Accept community shelter");
    }
}
