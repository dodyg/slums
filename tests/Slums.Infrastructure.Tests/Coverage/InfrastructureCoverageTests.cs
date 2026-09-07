using FluentAssertions;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Infrastructure.Persistence;
using Slums.Infrastructure.Randomness;
using TUnit.Core;

namespace Slums.Infrastructure.Tests.Coverage;

internal sealed class InfrastructureCoverageTests
{
    [Test]
    public void SaveValueParser_ShouldParseDeclaredEnumsCaseSensitively()
    {
        SaveValueParser.ParseEnum<WeatherType>("Clear", "weather").Should().Be(WeatherType.Clear);
        var act = () => SaveValueParser.ParseEnum<WeatherType>("clear", "weather");

        act.Should().Throw<InvalidDataException>().WithMessage("*weather*clear*");
    }

    [Test]
    public void SnapshotValidator_ShouldAcceptACapturedSessionWithoutProblems()
    {
        var problems = new List<string>();

        SaveGameSnapshotValidator.Validate(GameSessionSnapshot.Capture(new GameSession()), problems);

        problems.Should().BeEmpty();
    }

    [Test]
    public void SnapshotValidator_ShouldReportInvalidPolicePressure()
    {
        var snapshot = GameSessionSnapshot.Capture(new GameSession()) with
        {
            Crime = new GameSessionCrimeSnapshot { PolicePressure = 101 }
        };
        var problems = new List<string>();

        SaveGameSnapshotValidator.Validate(snapshot, problems);

        problems.Should().Contain(problem => problem.Contains("police pressure", StringComparison.Ordinal));
    }

    [Test]
    public void SeededRandomSource_ShouldProduceTheSameSequenceForTheSameSeed()
    {
#pragma warning disable CA5394 // Deterministic gameplay randomness is intentional.
        var first = new SeededRandomSource(42).SharedRandom;
        var second = new SeededRandomSource(42).SharedRandom;

        Enumerable.Range(0, 8).Select(_ => first.Next()).Should().Equal(Enumerable.Range(0, 8).Select(_ => second.Next()));
#pragma warning restore CA5394
    }
}
