using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeServiceTests
{
    [Test]
    public void AttemptCrime_ShouldSucceedFrequently_WhenRiskAndPressureAreLow()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);
        var random = new Random(1234);

        var successes = 0;
        for (var i = 0; i < 200; i++)
        {
            if (service.AttemptCrime(attempt, player, 0, random).Success)
            {
                successes++;
            }
        }

        successes.Should().BeGreaterThan(120);
    }

    [Test]
    public void AttemptCrime_ShouldDetectFrequently_WhenRiskAndPressureAreHigh()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 55, 25, 0, 10);
        var random = new Random(4321);

        var detections = 0;
        for (var i = 0; i < 200; i++)
        {
            if (service.AttemptCrime(attempt, player, 90, random).Detected)
            {
                detections++;
            }
        }

        detections.Should().BeGreaterThan(140);
    }
}