using Slums.Core.Characters;
using Slums.Core.Skills;

namespace Slums.Core.Crimes;

public sealed class CrimeService
{
#pragma warning disable CA1822
    public CrimeResolutionPreview PreviewCrime(CrimeAttempt attempt, PlayerCharacter player, int policePressure)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(player);

        var streetSmartsBonus = player.Skills.GetLevel(SkillId.StreetSmarts) >= 3 ? 10 : 0;
        var detectionChance = Math.Clamp(attempt.DetectionRisk + (policePressure / 2) - streetSmartsBonus, 5, 95);
        var successChance = Math.Clamp(90 - attempt.DetectionRisk - (policePressure / 3) + streetSmartsBonus, 10, 95);

        return new CrimeResolutionPreview(
            detectionChance,
            successChance,
            attempt.PolicePressureIncrease,
            Math.Max(1, attempt.PolicePressureIncrease / 3));
    }

#pragma warning disable CA1822
    public CrimeResult AttemptCrime(CrimeAttempt attempt, PlayerCharacter player, int policePressure, Random random)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(random);

        if (player.Stats.Energy < attempt.EnergyCost)
        {
            return new CrimeResult
            {
                Message = $"Too exhausted for {attempt.Name}.",
                EnergyCost = 0,
                StressCost = 0,
                PolicePressureDelta = 0
            };
        }

        var preview = PreviewCrime(attempt, player, policePressure);
    #pragma warning disable CA5394
        var detectionChance = preview.DetectionChance;
        var successChance = preview.SuccessChance;
        var success = random.Next(100) < successChance;
        var detected = random.Next(100) < detectionChance;
        var moneyEarned = success ? Math.Max(0, attempt.BaseReward + random.Next(-5, 11)) : 0;
    #pragma warning restore CA5394
        var stressCost = success ? (detected ? 12 : 6) : (detected ? 18 : 10);
        var policePressureDelta = detected ? preview.PolicePressureIfDetected : preview.PolicePressureIfUndetected;
        var arrestWarning = detected && policePressure + policePressureDelta >= 80;

        return new CrimeResult
        {
            Success = success,
            Detected = detected,
            ArrestWarning = arrestWarning,
            MoneyEarned = moneyEarned,
            EnergyCost = attempt.EnergyCost,
            StressCost = stressCost,
            PolicePressureDelta = policePressureDelta,
            Message = BuildMessage(attempt, success, detected, moneyEarned)
        };
    }

    private static string BuildMessage(CrimeAttempt attempt, bool success, bool detected, int moneyEarned)
    {
        if (success && detected)
        {
            return $"{attempt.Name} paid {moneyEarned} LE, but the police took notice.";
        }

        if (success)
        {
            return $"{attempt.Name} worked. You made {moneyEarned} LE.";
        }

        if (detected)
        {
            return $"{attempt.Name} went bad. You got nothing and attention followed you home.";
        }

        return $"{attempt.Name} failed. You come away empty-handed.";
    }
}