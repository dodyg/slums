using FluentAssertions;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Infrastructure.Persistence;
using Slums.Infrastructure.Randomness;
using TUnit.Core;
using Slums.TestSupport;

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

        SaveGameSnapshotValidator.Validate(GameSessionSnapshot.Capture(TestSessions.Create()), TestContent.Catalog, problems);

        problems.Should().BeEmpty();
    }

    [Test]
    public void SnapshotValidator_ShouldReportInvalidPolicePressure()
    {
        var snapshot = GameSessionSnapshot.Capture(TestSessions.Create()) with
        {
            Crime = new GameSessionCrimeSnapshot { PolicePressure = 101 }
        };
        var problems = new List<string>();

        SaveGameSnapshotValidator.Validate(snapshot, TestContent.Catalog, problems);

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
