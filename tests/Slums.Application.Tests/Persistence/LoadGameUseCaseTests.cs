using FluentAssertions;
using NSubstitute;
using Slums.Application.Persistence;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Persistence;

internal sealed class LoadGameUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldReturnLoadedState_FromStore()
    {
        var store = Substitute.For<ISaveGameStore>();
        var expectedState = new GameState();
        var loadedState = new LoadedGameState(
            "slot1",
            "checkpoint",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            "intro_medical",
            expectedState);
        store.LoadAsync("slot1", Arg.Any<CancellationToken>()).Returns(loadedState);
        var useCase = new LoadGameUseCase(store);

        var result = await useCase.ExecuteAsync("slot1").ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Should().BeSameAs(loadedState);
        result!.GameState.Should().BeSameAs(expectedState);
    }
}