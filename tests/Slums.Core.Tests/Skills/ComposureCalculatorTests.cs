using FluentAssertions;
using Slums.Core.Skills;
using TUnit.Core;

namespace Slums.Core.Tests.Skills;

internal sealed class ComposureCalculatorTests
{
    [Test]
    public void WorkMistakeThreshold_ShouldImproveAtFirstMeaningfulLevel()
    {
        ComposureCalculator.GetWorkMistakeStressThreshold(0, 60).Should().Be(60);
        ComposureCalculator.GetWorkMistakeStressThreshold(2, 60).Should().Be(65);
        ComposureCalculator.GetWorkMistakeStressThreshold(10, 60).Should().Be(65);
    }

    [Test]
    public void DebtStress_ShouldRemainPresentButEaseAtAdvancedLevels()
    {
        ComposureCalculator.GetDebtStressCost(0, 8).Should().Be(8);
        ComposureCalculator.GetDebtStressCost(4, 8).Should().Be(7);
        ComposureCalculator.GetDebtStressCost(8, 8).Should().Be(6);
        ComposureCalculator.GetDebtStressCost(10, 1).Should().Be(0);
    }

    [Test]
    public void CrisisRelief_ShouldOnlyApplyAtHighThresholds()
    {
        ComposureCalculator.GetCrisisStressRelief(0, 4).Should().Be(0);
        ComposureCalculator.GetCrisisStressRelief(6, 4).Should().Be(2);
        ComposureCalculator.GetCrisisStressRelief(8, 4).Should().Be(3);
        ComposureCalculator.GetCrisisStressRelief(10, 1).Should().Be(1);
    }
}
