using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Slums.Application.Persistence;
using TUnit.Core;

namespace Slums.Application.Tests.Persistence;

internal sealed class LoadGameUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldReturnLoadedResult_FromStore()
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
        store.LoadAsync("slot1", Arg.Any<CancellationToken>()).Returns(LoadGameResult.Loaded(loadedSession));
        var useCase = new LoadGameUseCase(store, NullLogger<LoadGameUseCase>.Instance);

        var result = await useCase.ExecuteAsync("slot1").ConfigureAwait(false);

        result.Kind.Should().Be(LoadGameResultKind.Loaded);
        result.Session.Should().BeSameAs(loadedSession);
        result.Session!.GameSession.Should().BeSameAs(expectedGameSession);
    }

    [Test]
    public async Task ExecuteAsync_ShouldPassThroughNonLoadedResults()
    {
        var store = Substitute.For<ISaveGameStore>();
        store.LoadAsync("slot1", Arg.Any<CancellationToken>()).Returns(LoadGameResult.Corrupt("bad json"));
        var useCase = new LoadGameUseCase(store, NullLogger<LoadGameUseCase>.Instance);

        var result = await useCase.ExecuteAsync("slot1").ConfigureAwait(false);

        result.Kind.Should().Be(LoadGameResultKind.Corrupt);
        result.Session.Should().BeNull();
        result.Detail.Should().Be("bad json");
    }

    [Test]
    public async Task ExecuteAsync_ShouldRejectInvalidSlot()
    {
        var store = Substitute.For<ISaveGameStore>();
        var useCase = new LoadGameUseCase(store, NullLogger<LoadGameUseCase>.Instance);

        var act = async () => await useCase.ExecuteAsync("../escape").ConfigureAwait(false);

        await act.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
        await store.DidNotReceiveWithAnyArgs().LoadAsync(default!, default).ConfigureAwait(false);
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

        loadedSession.Invoking(static session => session.GameSession)
            .Should()
            .Throw<InvalidOperationException>();
        loadedSession.Invoking(static session => session.TakeGameSession())
            .Should()
            .Throw<InvalidOperationException>();
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
