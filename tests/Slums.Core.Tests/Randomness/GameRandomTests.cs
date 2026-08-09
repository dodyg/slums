using FluentAssertions;
using Slums.Core.Randomness;
using TUnit;

namespace Slums.Core.Tests.Randomness;

internal sealed class GameRandomTests
{
    [Test]
    public async Task SameSeed_ProducesIdenticalSequence()
    {
        var first = new GameRandom(12345);
        var second = new GameRandom(12345);

        var firstSequence = Enumerable.Range(0, 1000).Select(_ => first.Next()).ToArray();
        var secondSequence = Enumerable.Range(0, 1000).Select(_ => second.Next()).ToArray();

        firstSequence.Should().Equal(secondSequence);
    }

    [Test]
    public async Task DifferentSeeds_ProduceDifferentSequences()
    {
        var first = new GameRandom(1);
        var second = new GameRandom(2);

        var firstValue = first.Next();
        var secondValue = second.Next();

        firstValue.Should().NotBe(secondValue);
    }

    [Test]
    public async Task CapturedState_ResumesExactSequence()
    {
        var original = new GameRandom(42);
        for (var i = 0; i < 100; i++)
        {
            original.Next();
        }

        var savedState = original.CaptureState();

        var expectedContinuation = Enumerable.Range(0, 500).Select(_ => original.Next()).ToArray();

        var restored = new GameRandom(savedState);
        var actualContinuation = Enumerable.Range(0, 500).Select(_ => restored.Next()).ToArray();

        actualContinuation.Should().Equal(expectedContinuation);
    }

    [Test]
    public async Task RestoreState_ResumesExactSequence()
    {
        var original = new GameRandom(42);
        for (var i = 0; i < 100; i++)
        {
            original.Next();
        }

        var savedState = original.CaptureState();
        var expectedContinuation = Enumerable.Range(0, 500).Select(_ => original.Next()).ToArray();

        var restored = new GameRandom(0);
        restored.RestoreState(savedState);
        var actualContinuation = Enumerable.Range(0, 500).Select(_ => restored.Next()).ToArray();

        actualContinuation.Should().Equal(expectedContinuation);
    }

    [Test]
    public async Task Next_RespectsDocumentedRanges()
    {
        var random = new GameRandom(7);

        for (var i = 0; i < 10_000; i++)
        {
            random.Next().Should().BeInRange(0, int.MaxValue - 1);
            random.Next(100).Should().BeInRange(0, 99);
            random.Next(5, 16).Should().BeInRange(5, 15);
            random.NextDouble().Should().BeInRange(0.0, 1.0);
        }
    }

    [Test]
    public async Task Next_MaxValue_IncludesZeroBoundary()
    {
        var random = new GameRandom(99);

        for (var i = 0; i < 10_000; i++)
        {
            random.Next(1).Should().Be(0);
        }
    }
}
