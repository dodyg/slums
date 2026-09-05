using FluentAssertions;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Technology;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Technology;

internal sealed class DigitalServiceTests
{
    [Test]
    public void BiometricAppealPreview_ShouldExposeUncertaintyAndObligation()
    {
        var session = CreateSession(6);

        var preview = session.PreviewDigitalService(DigitalServiceActionType.SubmitBiometricAppeal);

        preview.CanPerform.Should().BeTrue();
        preview.SuccessChance.Should().Be(60);
        preview.CreatesObligation.Should().BeTrue();
        preview.Action.MoneyCost.Should().Be(5);
        preview.Action.TimeCostMinutes.Should().Be(90);
    }

    [Test]
    public void BiometricAppeal_ShouldConsumeResourcesAndLeaveReviewPending()
    {
        var session = CreateSession(6);
        var beforeMoney = session.Player.Stats.Money;
        var beforeEnergy = session.Player.Stats.Energy;

        var result = session.PerformDigitalService(DigitalServiceActionType.SubmitBiometricAppeal);

        result.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(beforeMoney - 5);
        session.Player.Stats.Energy.Should().Be(beforeEnergy - 8);
        session.Technology.BiometricAppealPending.Should().BeTrue();
        session.Technology.HandsetDataExposure.Should().Be(2);
        session.Mutations[^1].Category.Should().Be("Technology");
    }

    [Test]
    public void BiometricAppeal_ShouldNotBeRepeatableWhileReviewIsPending()
    {
        var session = CreateSession(10);
        session.Technology.RecordBiometricAppeal();

        var preview = session.PreviewDigitalService(DigitalServiceActionType.SubmitBiometricAppeal);

        preview.CanPerform.Should().BeFalse();
        preview.UnavailabilityReason.Should().Be("A biometric review is already pending.");
    }

    [Test]
    public void DigitalLiteracyCalculator_ShouldKeepLegitimateAccessBounded()
    {
        DigitalLiteracyCalculator.GetCreditRefillCost(0, 5).Should().Be(5);
        DigitalLiteracyCalculator.GetCreditRefillCost(2, 5).Should().Be(4);
        DigitalLiteracyCalculator.GetCreditRefillCost(6, 5).Should().Be(3);
        DigitalLiteracyCalculator.GetBiometricAppealSuccessChance(10).Should().Be(85);
    }

    [Test]
    public void PhoneRefill_ShouldUseTheDigitalLiteracyWalletDiscount()
    {
        var session = CreateSession(2);
        session.Player.Stats.SetMoney(10);

        var result = session.RefillPhoneCredit();

        result.Success.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(6);
    }

    private static GameSession CreateSession(int skill)
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.CyberHacking, skill);
        session.Player.Stats.SetMoney(100);
        session.Player.Stats.SetEnergy(100);
        session.World.TravelTo(LocationId.Home);
        session.Clock.SetTime(1, 18, 0);
        return session;
    }
}
