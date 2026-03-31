using FluentAssertions;
using Slums.Core.Calendar;
using TUnit.Core;

namespace Slums.Core.Tests.Calendar;

internal sealed class RamadanStateTests
{
    [Test]
    public async Task Inactive_ShouldReturnInactiveState()
    {
        var state = RamadanState.Inactive;

        await Assert.That(state.IsActive).IsFalse();
        await Assert.That(state.PlayerIsFasting).IsFalse();
        await Assert.That(state.DaysFasting).IsEqualTo(0);
        await Assert.That(state.DaysRemaining).IsEqualTo(0);
    }

    [Test]
    public async Task WithFastingChoice_WhenFasting_ShouldSetPlayerIsFasting()
    {
        var state = RamadanState.Inactive.WithFastingChoice(true);

        await Assert.That(state.PlayerIsFasting).IsTrue();
    }

    [Test]
    public async Task AdvanceDay_WhenInactive_ShouldReturnSameState()
    {
        var state = RamadanState.Inactive;

        var advanced = state.AdvanceDay();

        await Assert.That(advanced.IsActive).IsFalse();
    }

    [Test]
    public async Task AdvanceDay_WhenActiveAndFasting_ShouldIncrementDaysFasting()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = true,
            DaysFasting = 5,
            DaysRemaining = 25
        };

        var advanced = state.AdvanceDay();

        await Assert.That(advanced.DaysFasting).IsEqualTo(6);
        await Assert.That(advanced.DaysRemaining).IsEqualTo(24);
    }

    [Test]
    public async Task AdvanceDay_WhenActiveAndNotFasting_ShouldNotIncrementDaysFasting()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = false,
            DaysFasting = 0,
            DaysRemaining = 25
        };

        var advanced = state.AdvanceDay();

        await Assert.That(advanced.DaysFasting).IsEqualTo(0);
    }

    [Test]
    public async Task EnergyModifier_WhenInactive_ShouldReturnZero()
    {
        var state = RamadanState.Inactive;

        await Assert.That(state.EnergyModifier).IsEqualTo(0);
    }

    [Test]
    public async Task EnergyModifier_WhenActiveAndFasting_ShouldReturnNegative()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = true
        };

        await Assert.That(state.EnergyModifier).IsEqualTo(-5);
    }

    [Test]
    public async Task StressModifier_WhenActiveAndFasting_ShouldReturnPositive()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = true
        };

        await Assert.That(state.StressModifier).IsEqualTo(3);
    }

    [Test]
    public async Task StressModifier_WhenActiveAndNotFasting_ShouldReturnLowerStress()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = false
        };

        await Assert.That(state.StressModifier).IsEqualTo(2);
    }

    [Test]
    public async Task TrustModifierWithReligiousNpcs_WhenFasting_ShouldReturnPositive()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = true
        };

        await Assert.That(state.TrustModifierWithReligiousNpcs).IsEqualTo(1);
    }

    [Test]
    public async Task TrustModifierWithReligiousNpcs_WhenNotFasting_ShouldReturnZero()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = false
        };

        await Assert.That(state.TrustModifierWithReligiousNpcs).IsEqualTo(0);
    }

    [Test]
    public async Task JobPayModifierPercent_WhenFasting_ShouldReturnNegative()
    {
        var state = new RamadanState
        {
            IsActive = true,
            PlayerIsFasting = true
        };

        await Assert.That(state.JobPayModifierPercent).IsEqualTo(-10);
    }
}
