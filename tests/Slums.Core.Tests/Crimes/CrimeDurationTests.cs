using FluentAssertions;
using Slums.Core.Crimes;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeDurationTests
{
    [Test]
    public void EveryCrimeType_ShouldHaveAVisibleDurationBetweenOneAndThreeHours()
    {
        foreach (var crimeType in Enum.GetValues<CrimeType>())
        {
            var attempt = new CrimeAttempt(crimeType, 1, 1, 1, 0, 0);

            attempt.DurationMinutes.Should().BeInRange(60, 180);
        }
    }
}
