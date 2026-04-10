using FluentAssertions;
using Slums.Game.Input;
using TUnit.Core;

namespace Slums.Game.Tests.Input;

internal sealed class ModalKeySuppressionTests
{
    [Test]
    public void SuppressActionKeysUntilRelease_ShouldBlockBothConfirmAndCancel()
    {
        var gate = new ScreenActionKeyGate();
        gate.SuppressActionKeysUntilRelease();

        gate.TryConsumeConfirm(isPressed: true).Should().BeFalse();
        gate.TryConsumeCancel(isPressed: true).Should().BeFalse();
    }

    [Test]
    public void AfterRelease_ConfirmAndCancelShouldBothWork()
    {
        var gate = new ScreenActionKeyGate();
        gate.SuppressActionKeysUntilRelease();

        gate.TryConsumeConfirm(isPressed: false).Should().BeFalse();
        gate.TryConsumeCancel(isPressed: false).Should().BeFalse();

        gate.TryConsumeConfirm(isPressed: true).Should().BeTrue();
        gate.TryConsumeCancel(isPressed: true).Should().BeTrue();
    }

    [Test]
    public void DoubleSuppress_ShouldStillOnlyRequireOneRelease()
    {
        var gate = new ScreenActionKeyGate();
        gate.SuppressActionKeysUntilRelease();
        gate.SuppressActionKeysUntilRelease();

        gate.TryConsumeConfirm(isPressed: false).Should().BeFalse();
        gate.TryConsumeConfirm(isPressed: true).Should().BeTrue();
    }

    [Test]
    public void IndependentConfirmAndCancelSuppression()
    {
        var gate = new ScreenActionKeyGate();
        gate.SuppressConfirmUntilRelease();

        gate.TryConsumeConfirm(isPressed: true).Should().BeFalse();
        gate.TryConsumeCancel(isPressed: true).Should().BeTrue();
    }
}
