using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Skills;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeServiceBoundaryTests
{
    [Test]
    public async Task AttemptCrime_ReturnsZeroCost_WhenEnergyTooLow()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Stats.SetEnergy(2);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        var result = service.AttemptCrime(attempt, player, 0, new Random(42));

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.EnergyCost).IsEqualTo(0);
        await Assert.That(result.StressCost).IsEqualTo(0);
        await Assert.That(result.PolicePressureDelta).IsEqualTo(0);
        await Assert.That(result.MoneyEarned).IsEqualTo(0);
    }

    [Test]
    public async Task AttemptCrime_ReturnsEnergyCost_WhenSuccessful()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Stats.SetEnergy(50);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        var result = service.AttemptCrime(attempt, player, 0, new Random(1));

        await Assert.That(result.EnergyCost).IsEqualTo(10);
    }

    [Test]
    public async Task AttemptCrime_SetsArrestWarning_WhenPressureThresholdReached()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 55, 25, 0, 10);
        var random = new Random(4321);

        CrimeResult? arrestedResult = null;
        for (var i = 0; i < 200; i++)
        {
            var result = service.AttemptCrime(attempt, player, 75, random);
            if (result.Detected && result.ArrestWarning)
            {
                arrestedResult = result;
                break;
            }
        }

        await Assert.That(arrestedResult).IsNotNull();
    }

    [Test]
    public async Task AttemptCrime_StressCost_DetectedSuccess()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 5, 10, 0, 10);
        var random = new Random(42);

        for (var i = 0; i < 300; i++)
        {
            var result = service.AttemptCrime(attempt, player, 0, random);
            if (result.Success && result.Detected)
            {
                await Assert.That(result.StressCost).IsEqualTo(12);
                return;
            }
        }
    }

    [Test]
    public async Task AttemptCrime_StressCost_UndetectedSuccess()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 5, 10, 0, 10);
        var random = new Random(42);

        for (var i = 0; i < 300; i++)
        {
            var result = service.AttemptCrime(attempt, player, 0, random);
            if (result.Success && !result.Detected)
            {
                await Assert.That(result.StressCost).IsEqualTo(6);
                return;
            }
        }
    }

    [Test]
    public async Task AttemptCrime_StressCost_DetectedFailure()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 55, 25, 0, 10);
        var random = new Random(4321);

        for (var i = 0; i < 300; i++)
        {
            var result = service.AttemptCrime(attempt, player, 90, random);
            if (!result.Success && result.Detected)
            {
                await Assert.That(result.StressCost).IsEqualTo(18);
                return;
            }
        }
    }

    [Test]
    public async Task AttemptCrime_StressCost_UndetectedFailure()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 55, 25, 0, 10);
        var random = new Random(4321);

        for (var i = 0; i < 300; i++)
        {
            var result = service.AttemptCrime(attempt, player, 90, random);
            if (!result.Success && !result.Detected)
            {
                await Assert.That(result.StressCost).IsEqualTo(10);
                return;
            }
        }
    }

    [Test]
    public async Task PreviewCrime_ClampsDetectionChanceToLowerBound()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Skills.SetLevel(SkillId.StreetSmarts, 3);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 5, 10, 0, 10);

        var preview = service.PreviewCrime(attempt, player, 0);

        await Assert.That(preview.DetectionChance).IsGreaterThanOrEqualTo(5);
    }

    [Test]
    public async Task PreviewCrime_ClampsDetectionChanceToUpperBound()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 80, 25, 0, 10);

        var preview = service.PreviewCrime(attempt, player, 90);

        await Assert.That(preview.DetectionChance).IsLessThanOrEqualTo(95);
    }

    [Test]
    public async Task PreviewCrime_ClampsSuccessChanceToLowerBound()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.Robbery, 70, 80, 25, 0, 10);

        var preview = service.PreviewCrime(attempt, player, 90);

        await Assert.That(preview.SuccessChance).IsGreaterThanOrEqualTo(10);
    }

    [Test]
    public async Task PreviewCrime_ClampsSuccessChanceToUpperBound()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Skills.SetLevel(SkillId.StreetSmarts, 3);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 5, 10, 0, 10);

        var preview = service.PreviewCrime(attempt, player, 0);

        await Assert.That(preview.SuccessChance).IsLessThanOrEqualTo(95);
    }

    [Test]
    public async Task PreviewCrime_MoneyEarnedVariesAroundBaseReward()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 5, 10, 0, 10);
        var random = new Random(42);
        var amounts = new HashSet<int>();

        for (var i = 0; i < 300; i++)
        {
            var result = service.AttemptCrime(attempt, player, 0, random);
            if (result.Success)
            {
                amounts.Add(result.MoneyEarned);
            }
        }

        await Assert.That(amounts.Count).IsGreaterThan(1);
    }

    [Test]
    public async Task BuildMessage_ContainsCrimeName()
    {
        var service = new CrimeService();
        var player = new PlayerCharacter();
        player.Stats.SetEnergy(50);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);
        var random = new Random(42);

        for (var i = 0; i < 100; i++)
        {
            var result = service.AttemptCrime(attempt, player, 0, random);
            await Assert.That(result.Message).Contains("Petty Theft");
        }
    }
}
