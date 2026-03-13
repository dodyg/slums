using FluentAssertions;
using NSubstitute;
using Slums.Application.Persistence;
using TUnit.Core;

namespace Slums.Application.Tests.Persistence;

internal sealed class LoadGameUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldReturnLoadedSession_FromStore()
    {
        var store = Substitute.For<ISaveGameStore>();
        using var loadedSession = LoadedGameSession.Create(
            "slot1",
            "checkpoint",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            "intro_medical",
            static () => new Slums.Core.State.GameSession());
        var expectedGameSession = loadedSession.GameSession;
        store.LoadAsync("slot1", Arg.Any<CancellationToken>()).Returns(loadedSession);
        var useCase = new LoadGameUseCase(store);

        var result = await useCase.ExecuteAsync("slot1").ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Should().BeSameAs(loadedSession);
        result!.GameSession.Should().BeSameAs(expectedGameSession);
    }

    [Test]
    public void LoadedGameSession_TakeGameSession_ShouldTransferOwnershipOnce()
    {
        using var loadedSession = LoadedGameSession.Create(
            "slot1",
            "checkpoint",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            "intro_medical",
            static () => new Slums.Core.State.GameSession());

        var gameSession = loadedSession.TakeGameSession();

        try
        {
            loadedSession.Invoking(static session => session.GameSession)
                .Should()
                .Throw<InvalidOperationException>();
            loadedSession.Invoking(static session => session.TakeGameSession())
                .Should()
                .Throw<InvalidOperationException>();
        }
        finally
        {
            gameSession.Dispose();
        }
    }

    [Test]
    public void LoadedGameSession_Dispose_ShouldRejectFurtherOwnershipTransfer()
    {
        var loadedSession = LoadedGameSession.Create(
            "slot1",
            "checkpoint",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            "intro_medical",
            static () => new Slums.Core.State.GameSession());

        loadedSession.Dispose();

        loadedSession.Invoking(static session => session.TakeGameSession())
            .Should()
            .Throw<ObjectDisposedException>();
    }
}
