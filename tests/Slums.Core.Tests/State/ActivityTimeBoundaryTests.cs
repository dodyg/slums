using Slums.Core.Jobs;
using Slums.Core.Phone;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class ActivityTimeBoundaryTests
{
    [Test]
    public async Task TravelAcrossEndOfDay_ShouldFinishAtHomeAfterNightlyReset()
    {
        using var session = new GameSession();
        session.Clock.SetTime(day: 1, hour: 21, minute: 50);

        var result = session.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(session.Clock.Day).IsEqualTo(2);
        await Assert.That(session.Clock.Hour).IsEqualTo(6);
        await Assert.That(session.Clock.Minute).IsEqualTo(35);
        await Assert.That(session.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task WorkAcrossEndOfDay_ShouldApplyTerritoryImpactToWorkDistrict()
    {
        using var session = new GameSession();
        session.Clock.SetTime(day: 1, hour: 20, minute: 0);
        session.World.TravelTo(LocationId.CallCenter);

        var result = session.WorkJob(JobRegistry.CallCenterWork, new Random(1));

        await Assert.That(result.Success).IsTrue();
        await Assert.That(session.Clock.Day).IsEqualTo(2);
        await Assert.That(session.World.CurrentLocationId).IsEqualTo(LocationId.Home);
        await Assert.That(session.Territory.GetControl(DistrictId.Dokki).Tension).IsEqualTo(27);
    }

    [Test]
    public async Task PhoneResponse_ShouldNotChargeMissedCallWhenThereIsNotEnoughTime()
    {
        using var session = new GameSession();
        session.Clock.SetTime(day: 1, hour: 21, minute: 30);
        var message = new PhoneMessage
        {
            Id = "late-call",
            Sender = "Mona",
            Content = "Call me back.",
            ResponseTimeCost = 1,
            WasMissed = true
        };
        session.PhoneMessages.AddMessage(message);
        var moneyBefore = session.Player.Stats.Money;

        var result = session.RespondToMessage(message.Id);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(moneyBefore);
        await Assert.That(session.PhoneMessages.GetMessage(message.Id)!.Responded).IsFalse();
    }

    [Test]
    public async Task PhoneResponse_ShouldValidateCombinedMissedCallAndResponseCostAtomically()
    {
        using var session = new GameSession();
        session.Player.Stats.ModifyMoney(-95);
        var message = new PhoneMessage
        {
            Id = "costly-call",
            Sender = "Mona",
            Content = "Bring the contribution.",
            ResponseMoneyCost = 5,
            WasMissed = true
        };
        session.PhoneMessages.AddMessage(message);

        var result = session.RespondToMessage(message.Id);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(5);
        await Assert.That(session.PhoneMessages.GetMessage(message.Id)!.Responded).IsFalse();
    }
}
