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

    [Test]
    public void PreviewCrime_ShouldReflectStreetSmartsAndPressureThresholds()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Skills.SetLevel(Slums.Core.Skills.SkillId.StreetSmarts, 3);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        var preview = service.PreviewCrime(attempt, player, policePressure: 60);

        preview.DetectionChance.Should().Be(37);
        preview.SuccessChance.Should().Be(61);
    }

    [Test]
    public void PreviewCrime_ShouldBoostNetworkErrandSuccess_WhenCyberHackingIsHigh()
    {
        var service = new CrimeService();
        var unskilled = new PlayerCharacter();
        var hacker = new PlayerCharacter();
        hacker.Skills.SetLevel(Slums.Core.Skills.SkillId.CyberHacking, 2);
        var attempt = new CrimeAttempt(CrimeType.NetworkErrand, 130, 50, 30, 0, 24);

        var baseline = service.PreviewCrime(attempt, unskilled, policePressure: 0);
        var boosted = service.PreviewCrime(attempt, hacker, policePressure: 0);

        boosted.SuccessChance.Should().Be(baseline.SuccessChance + 8);
        boosted.DetectionChance.Should().Be(baseline.DetectionChance);
    }

    [Test]
    public void PreviewCrime_ShouldNotBoostOtherCrimes_WhenCyberHackingIsHigh()
    {
        var service = new CrimeService();
        var unskilled = new PlayerCharacter();
        var hacker = new PlayerCharacter();
        hacker.Skills.SetLevel(Slums.Core.Skills.SkillId.CyberHacking, 5);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        var baseline = service.PreviewCrime(attempt, unskilled, policePressure: 0);
        var boosted = service.PreviewCrime(attempt, hacker, policePressure: 0);

        boosted.SuccessChance.Should().Be(baseline.SuccessChance);
        boosted.DetectionChance.Should().Be(baseline.DetectionChance);
    }
}
