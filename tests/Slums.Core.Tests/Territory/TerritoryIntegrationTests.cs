using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Heat;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Territory;

internal sealed class TerritoryIntegrationTests
{
    [Test]
    public async Task GameSession_Territory_IsInitializedOnConstruction()
    {
        using var session = new GameSession(new Random(42));

        await Assert.That(session.Territory.IsInitialized).IsTrue();
    }

    [Test]
    public async Task GameSession_Territory_HasAllDistricts()
    {
        using var session = new GameSession(new Random(42));

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            var control = session.Territory.GetControl(district);
            await Assert.That(control.FactionInfluence).IsNotEmpty();
        }
    }

    [Test]
    public async Task GameSession_EndDay_AppliesTerritoryDecay()
    {
        using var session = new GameSession(new Random(42));
        session.Territory.ModifyTension(DistrictId.Imbaba, 30);

        var before = session.Territory.GetControl(DistrictId.Imbaba).Tension;
        session.EndDay(new Random(42));
        var after = session.Territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsLessThan(before);
    }

    [Test]
    public async Task GameSession_EndDay_DangerousTension_AddsHeat()
    {
        using var session = new GameSession(new Random(42));
        session.Territory.ModifyTension(DistrictId.Imbaba, 60);

        var before = session.DistrictHeat.GetHeat(DistrictId.Imbaba);
        session.EndDay(new Random(42));
        var after = session.DistrictHeat.GetHeat(DistrictId.Imbaba);

        await Assert.That(after).IsGreaterThan(before);
    }

    [Test]
    public async Task GameSession_GetFoodCost_IncludesTerritoryModifier_WhenHighTension()
    {
        using var normal = new GameSession(new Random(42));
        normal.World.TravelTo(LocationId.Home);

        using var highTension = new GameSession(new Random(42));
        highTension.World.TravelTo(LocationId.Home);
        highTension.Territory.ModifyTension(highTension.World.CurrentDistrict, 40);

        var normalCost = normal.GetFoodCost();
        var highCost = highTension.GetFoodCost();

        await Assert.That(highCost).IsGreaterThan(normalCost);
    }

    [Test]
    public async Task GameSession_GetStreetFoodCost_IncludesTerritoryModifier_WhenHighTension()
    {
        using var normal = new GameSession(new Random(42));
        normal.World.TravelTo(LocationId.Home);

        using var highTension = new GameSession(new Random(42));
        highTension.World.TravelTo(LocationId.Home);
        highTension.Territory.ModifyTension(highTension.World.CurrentDistrict, 40);

        var normalCost = normal.GetStreetFoodCost();
        var highCost = highTension.GetStreetFoodCost();

        await Assert.That(highCost).IsGreaterThan(normalCost);
    }

    [Test]
    public async Task GameSession_GetAvailableCrimes_ReturnsEmpty_WhenDangerousTension()
    {
        using var session = new GameSession(new Random(42));
        session.World.TravelTo(LocationId.Market);
        session.Player.Stats.SetMoney(100);
        session.Territory.ModifyTension(DistrictId.Imbaba, 60);

        var crimes = session.GetAvailableCrimes();

        await Assert.That(crimes).IsEmpty();
    }

    [Test]
    public async Task GameSession_CommitCrime_IncreasesTension()
    {
        using var session = new GameSession(new Random(42));
        session.World.TravelTo(LocationId.Market);
        session.Player.Stats.SetMoney(100);

        var crimes = session.GetAvailableCrimes();
        if (crimes.Count == 0)
        {
            return;
        }

        var before = session.Territory.GetControl(DistrictId.Imbaba).Tension;
        session.CommitCrime(crimes[0], new Random(42));
        var after = session.Territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsGreaterThan(before);
    }

    [Test]
    public async Task GameSession_WorkJob_ReducesTerritoryTension()
    {
        using var session = new GameSession(new Random(42));
        session.World.TravelTo(LocationId.Market);
        session.Player.Stats.SetMoney(100);
        session.Territory.ModifyTension(DistrictId.Imbaba, 20);

        var jobs = session.GetAvailableJobs();
        if (jobs.Count == 0)
        {
            return;
        }

        var before = session.Territory.GetControl(DistrictId.Imbaba).Tension;
        var result = session.WorkJob(jobs[0], new Random(42));
        if (result.Success)
        {
            var after = session.Territory.GetControl(DistrictId.Imbaba).Tension;
            await Assert.That(after).IsLessThan(before);
        }
    }

    [Test]
    public async Task TerritoryEventRegistry_StreetArgument_HasCorrectType()
    {
        var evt = TerritoryEventRegistry.StreetArgument;

        await Assert.That(evt.Type).IsEqualTo(TerritoryEventType.StreetArgument);
        await Assert.That(evt.StressModifier).IsGreaterThan(0);
    }

    [Test]
    public async Task TerritoryEventRegistry_PoliceCrackdown_BlocksCrime()
    {
        var evt = TerritoryEventRegistry.PoliceCrackdownEvent;

        await Assert.That(evt.BlocksCrime).IsTrue();
        await Assert.That(evt.TensionModifier).IsLessThan(0);
    }

    [Test]
    public async Task TerritoryEventRegistry_Crossfire_DamagesHealth()
    {
        var evt = TerritoryEventRegistry.CrossfireEvent;

        await Assert.That(evt.HealthModifier).IsLessThan(0);
        await Assert.That(evt.StressModifier).IsGreaterThan(0);
    }

    [Test]
    public async Task TerritoryEventRegistry_RefugeeSolidarity_ReducesStress()
    {
        var evt = TerritoryEventRegistry.RefugeeSolidarityEvent;

        await Assert.That(evt.StressModifier).IsLessThan(0);
        await Assert.That(evt.TensionModifier).IsLessThan(0);
    }

    [Test]
    public async Task TerritoryEventRegistry_TerritoryFlip_HasCorrectType()
    {
        var evt = TerritoryEventRegistry.TerritoryFlipEvent(FactionId.DokkiThugs);

        await Assert.That(evt.Type).IsEqualTo(TerritoryEventType.TerritoryFlip);
        await Assert.That(evt.Description).Contains("DokkiThugs");
    }

    [Test]
    public async Task TerritoryEventRegistry_ProtectionDemand_CostsMoney()
    {
        var evt = TerritoryEventRegistry.CreateProtectionDemand(50);

        await Assert.That(evt.MoneyModifier).IsLessThan(0);
        await Assert.That(evt.Description).Contains("50");
    }
}
