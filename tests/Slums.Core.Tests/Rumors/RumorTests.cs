using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.Rumors;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Rumors;

internal sealed class RumorTests
{
    [Test]
    public async Task Rumor_Decay_ReducesIntensity()
    {
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId> { NpcId.NeighborMona }, -2, []);

        rumor.Decay();

        await Assert.That(rumor.Intensity).IsEqualTo(3);
        await Assert.That(rumor.Age).IsEqualTo(1);
    }

    [Test]
    public async Task Rumor_PositiveDecaysFaster()
    {
        var rumor = new Rumor(RumorId.DevotedDaughter, "test", DistrictId.Imbaba, 1, 3, true,
            new HashSet<NpcId> { NpcId.NeighborMona }, 1, []);

        rumor.Decay();

        await Assert.That(rumor.Intensity).IsEqualTo(0);
    }

    [Test]
    public async Task Rumor_IsExpired_WhenIntensityZero()
    {
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 2, false,
            new HashSet<NpcId>(), -2, []);

        rumor.Decay();

        await Assert.That(rumor.IsExpired).IsTrue();
    }

    [Test]
    public async Task Rumor_IsExpired_WhenAgeExceeds4()
    {
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 10, false,
            new HashSet<NpcId>(), -2, []);

        for (var i = 0; i < 5; i++)
        {
            rumor.Decay();
        }

        await Assert.That(rumor.IsExpired).IsTrue();
    }

    [Test]
    public async Task RumorState_AddAndRetrieve()
    {
        var state = new RumorState();
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId>(), -2, []);

        state.AddRumor(rumor);

        await Assert.That(state.ActiveRumors.Count).IsEqualTo(1);
    }

    [Test]
    public async Task RumorState_RemoveExpired_ClearsExpiredRumors()
    {
        var state = new RumorState();
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 1, false,
            new HashSet<NpcId>(), -2, []);

        state.AddRumor(rumor);
        rumor.Decay();
        state.RemoveExpired();

        await Assert.That(state.ActiveRumors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RumorState_DecayAll_DecaysAllRumors()
    {
        var state = new RumorState();
        var r1 = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId>(), -2, []);
        var r2 = new Rumor(RumorId.RentUnpaid, "test", DistrictId.Imbaba, 1, 3, false,
            new HashSet<NpcId>(), -1, []);

        state.AddRumor(r1);
        state.AddRumor(r2);
        state.DecayAll();

        await Assert.That(r1.Intensity).IsEqualTo(3);
        await Assert.That(r2.Intensity).IsEqualTo(1);
    }

    [Test]
    public async Task RumorState_GetRumorsAffectingNpc_FiltersCorrectly()
    {
        var state = new RumorState();
        var affected = new HashSet<NpcId> { NpcId.NeighborMona };
        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            affected, -2, []);

        state.AddRumor(rumor);

        var monaRumors = state.GetRumorsAffectingNpc(NpcId.NeighborMona);
        var otherRumors = state.GetRumorsAffectingNpc(NpcId.NurseSalma);

        await Assert.That(monaRumors.Count).IsEqualTo(1);
        await Assert.That(otherRumors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RumorGenerator_CrimeSuccess_CreatesRumor()
    {
        var rumor = RumorGenerator.OnCrimeSuccess(DistrictId.Imbaba, 1);

        await Assert.That(rumor.Id).IsEqualTo(RumorId.CrimeSuccess);
        await Assert.That(rumor.IsPositive).IsFalse();
        await Assert.That(rumor.Intensity).IsGreaterThan(0);
        await Assert.That(rumor.AffectedNpcs.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task RumorGenerator_CrimeDetected_HasHigherIntensity()
    {
        var success = RumorGenerator.OnCrimeSuccess(DistrictId.Imbaba, 1);
        var detected = RumorGenerator.OnCrimeDetected(DistrictId.Imbaba, 1);

        await Assert.That(detected.Intensity).IsGreaterThan(success.Intensity);
    }

    [Test]
    public async Task RumorGenerator_RentUnpaid_CreatesRumorInImbaba()
    {
        var rumor = RumorGenerator.OnRentUnpaid(1);

        await Assert.That(rumor.Id).IsEqualTo(RumorId.RentUnpaid);
        await Assert.That(rumor.District).IsEqualTo(DistrictId.Imbaba);
    }

    [Test]
    public async Task RumorPropagator_SpreadsToNpcsOnDay1()
    {
        var rumorState = new RumorState();
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.NeighborMona, 30, 0);

        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId> { NpcId.NeighborMona }, -2, []);
        rumorState.AddRumor(rumor);

        RumorPropagator.Propagate(rumorState, relationships, 1);

        await Assert.That(rumor.NpcsWhoHeard.Contains(NpcId.NeighborMona)).IsTrue();
    }

    [Test]
    public async Task RumorPropagator_AppliesTrustModification()
    {
        var rumorState = new RumorState();
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.NeighborMona, 0, 0);

        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId> { NpcId.NeighborMona }, -2, []);
        rumorState.AddRumor(rumor);

        RumorPropagator.Propagate(rumorState, relationships, 1);

        await Assert.That(relationships.GetNpcRelationship(NpcId.NeighborMona).Trust).IsLessThan(0);
    }

    [Test]
    public async Task RumorPropagator_HighTrustNpcsReduceNegativeEffect()
    {
        var rumorState = new RumorState();
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.NeighborMona, 50, 0);

        var rumor = new Rumor(RumorId.CrimeSuccess, "test", DistrictId.Imbaba, 1, 5, false,
            new HashSet<NpcId> { NpcId.NeighborMona }, -4, []);
        rumorState.AddRumor(rumor);

        RumorPropagator.Propagate(rumorState, relationships, 1);

        var trust = relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;
        await Assert.That(trust).IsGreaterThan(-4);
    }

    [Test]
    public async Task GameSession_EndDay_GeneratesRumorOnSkips()
    {
        using var state = new GameSession(new Random(42));
        state.Player.Nutrition.Eat(MealQuality.Basic);
        state.RestoreWeather(WeatherType.Clear);

        for (var i = 0; i < 4; i++)
        {
            state.EndDay(new Random(42));
        }

        await Assert.That(state.EventAttendance.ConsecutiveSkips).IsGreaterThanOrEqualTo(3);
        await Assert.That(state.Rumors.ActiveRumors.Count).IsGreaterThan(0);
    }
}
