using FluentAssertions;
using Slums.Core.Technology;
using TUnit.Core;

namespace Slums.Core.Tests.Technology;

internal sealed class TechnologyObligationStateTests
{
    [Test]
    public void UsefulDigitalServices_ShouldAccumulateBoundedObligations()
    {
        var state = new TechnologyObligationState();

        state.RecordHandsetUse(120);
        state.RecordMicrogridRepair(12, 8);
        state.RecordTransitPermitReview();
        state.RecordBiometricAppeal();
        state.RecordTelemedicineTriage(4).Should().BeTrue();

        state.HandsetDataExposure.Should().Be(100);
        state.MicrogridRepairDebt.Should().Be(12);
        state.MicrogridStorageCondition.Should().Be(78);
        state.TransitPermitReview.Should().BeTrue();
        state.BiometricAppealPending.Should().BeTrue();
        state.LastTelemedicineTriageDay.Should().Be(4);
    }

    [Test]
    public void Telemedicine_ShouldNotDoubleCountTheSameDay()
    {
        var state = new TechnologyObligationState();

        state.RecordTelemedicineTriage(8).Should().BeTrue();
        state.RecordTelemedicineTriage(8).Should().BeFalse();
        state.RecordTelemedicineTriage(9).Should().BeTrue();
    }
}
