using FluentAssertions;
using NSubstitute;
using Slums.Application.Persistence;
using Slums.Application.Randomness;
using Slums.Core.State;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Application.Tests.Persistence;

internal sealed class NewGameUseCaseTests
{
    [Test]
    public void Execute_ShouldCreateSessionBackedBySharedRandom()
    {
        var sharedRandom = new Random(2060);
        var randomSource = Substitute.For<IRandomSource>();
        randomSource.SharedRandom.Returns(sharedRandom);
        var useCase = new NewGameUseCase(randomSource, contentCatalogProvider: TestContent.CreateProvider());

        var session = useCase.Execute();

        session.Should().NotBeNull();
        session.IsGameOver.Should().BeFalse();
        session.Clock.Day.Should().Be(1);
    }

    [Test]
    public void Constructor_ShouldThrow_WhenRandomSourceIsNull()
    {
        var act = () => new NewGameUseCase(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("randomSource");
    }
}
