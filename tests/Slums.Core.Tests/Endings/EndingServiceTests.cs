using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingServiceTests
{
    [Test]
    public async Task CheckEndings_ShouldReturnStabilityHonestWork_WhenCriteriaMet()
    {
        var state = new GameState();
        state.Player.Stats.SetMoney(250);
        state.SetDaysSurvived(30);
        state.SetPolicePressure(10);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.StabilityHonestWork);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNull_WhenCriteriaAreNotMet()
    {
        var state = new GameState();

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ShouldReturnArrested_WhenPolicePressureHitsMaximum()
    {
        var state = new GameState();
        state.SetPolicePressure(100);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }
}