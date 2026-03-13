using FluentAssertions;
using NSubstitute;
using Slums.Application.Persistence;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Persistence;

internal sealed class SaveGameUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldForwardSaveRequestToStore()
    {
        var store = Substitute.For<ISaveGameStore>();
        var useCase = new SaveGameUseCase(store);
        using var gameSession = new GameSession();
        var request = SaveGameRequest.Create(gameSession, "intro_medical");

        await useCase.ExecuteAsync(request, "slot1").ConfigureAwait(false);

        await store.Received(1).SaveAsync(
            Arg.Is<SaveGameRequest>(saved => ReferenceEquals(saved, request)),
            "slot1",
            Arg.Any<CancellationToken>()).ConfigureAwait(false);
    }

    [Test]
    public async Task ExecuteAsync_ShouldThrow_WhenRequestIsNull()
    {
        var store = Substitute.For<ISaveGameStore>();
        var useCase = new SaveGameUseCase(store);

        var act = () => useCase.ExecuteAsync(null!, "slot1");

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public void Create_ShouldCaptureCheckpointNameAndNarrativeProgress()
    {
        using var gameSession = new GameSession();
        gameSession.Player.Name = "Aya";

        var request = SaveGameRequest.Create(gameSession, "intro_medical");

        request.GameSession.Should().BeSameAs(gameSession);
        request.LastKnot.Should().Be("intro_medical");
        request.CheckpointName.Should().Contain("Day 1");
    }
}
