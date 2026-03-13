using FluentAssertions;
using Slums.Game.Input;
using TUnit.Core;

namespace Slums.Application.Tests.Input;

internal sealed class ScreenActionKeyGateTests
{
    [Test]
    public void TryConsumeConfirm_ShouldOnlyFireOnceUntilReleased()
    {
        var gate = new ScreenActionKeyGate();

        gate.TryConsumeConfirm(isPressed: true).Should().BeTrue();
        gate.TryConsumeConfirm(isPressed: true).Should().BeFalse();
        gate.TryConsumeConfirm(isPressed: false).Should().BeFalse();
        gate.TryConsumeConfirm(isPressed: true).Should().BeTrue();
    }

    [Test]
    public void SuppressActionKeysUntilRelease_ShouldIgnoreHeldConfirmUntilReleased()
    {
        var gate = new ScreenActionKeyGate();

        gate.SuppressActionKeysUntilRelease();

        gate.TryConsumeConfirm(isPressed: true).Should().BeFalse();
        gate.TryConsumeConfirm(isPressed: false).Should().BeFalse();
        gate.TryConsumeConfirm(isPressed: true).Should().BeTrue();
    }

    [Test]
    public void SuppressActionKeysUntilRelease_ShouldIgnoreHeldCancelUntilReleased()
    {
        var gate = new ScreenActionKeyGate();

        gate.SuppressActionKeysUntilRelease();

        gate.TryConsumeCancel(isPressed: true).Should().BeFalse();
        gate.TryConsumeCancel(isPressed: false).Should().BeFalse();
        gate.TryConsumeCancel(isPressed: true).Should().BeTrue();
    }
}
