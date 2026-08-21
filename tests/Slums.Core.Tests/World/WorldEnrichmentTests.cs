using Slums.Core.Clock;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Core.World;
using Slums.Core.World.News;
using TUnit.Core;

namespace Slums.Core.Tests.World;

internal sealed class WorldEnrichmentTests
{
    [Test]
    public async Task Infrastructure_ShouldKeepTheStrongerLongerDisruption()
    {
        var state = new InfrastructureState();
        state.StartDisruption(DistrictId.Imbaba, InfrastructureServiceType.Water, InfrastructureSeverity.Strained, 2, 4, "first");
        state.StartDisruption(DistrictId.Imbaba, InfrastructureServiceType.Water, InfrastructureSeverity.Disrupted, 5, 5, "second");

        var service = state.Get(DistrictId.Imbaba, InfrastructureServiceType.Water);
        await Assert.That(service.Severity).IsEqualTo(InfrastructureSeverity.Disrupted);
        await Assert.That(service.RemainingDays).IsEqualTo(5);
        await Assert.That(InfrastructureImpactCalculator.GetFoodStressModifier(state, DistrictId.Imbaba)).IsEqualTo(3);
    }

    [Test]
    public async Task Infrastructure_ShouldRecoverAfterRemainingDaysExpire()
    {
        var state = new InfrastructureState();
        state.StartDisruption(DistrictId.Dokki, InfrastructureServiceType.Transport, InfrastructureSeverity.Strained, 2, 1, "route");

        state.AdvanceDay();
        await Assert.That(state.Get(DistrictId.Dokki, InfrastructureServiceType.Transport).RemainingDays).IsEqualTo(1);
        state.AdvanceDay();
        await Assert.That(state.Get(DistrictId.Dokki, InfrastructureServiceType.Transport).Severity).IsEqualTo(InfrastructureSeverity.Normal);
    }

    [Test]
    public async Task NewsState_ShouldExpireAndAllowOnlyOneResponse()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "test_news",
            Headline = "A test headline",
            Body = "A test body",
            SourceLabel = "Test source",
            DurationDays = 2,
            Responses = [new NewsResponseDefinition { Id = "help", Label = "Help", Type = NewsResponseType.HelpCommunity }]
        };
        var state = new NewsState();
        state.Activate(definition, 4);

        await Assert.That(state.TryUseResponse(definition.Id, "help")).IsTrue();
        await Assert.That(state.TryUseResponse(definition.Id, "help-again")).IsFalse();
        state.BeginDay(6);
        await Assert.That(state.ActiveFlashes).IsEmpty();
    }

    [Test]
    public async Task NewsService_ShouldSelectWeightedDefinitionAndStartInfrastructureEffect()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "route_news",
            Headline = "A route slows",
            Body = "A route slows across the city.",
            SourceLabel = "Depot",
            MinimumDay = 1,
            Weight = 1,
            DurationDays = 3,
            AffectedDistricts = [DistrictId.BulaqAlDakrour],
            Effects = [new NewsEffectDefinition
            {
                Type = NewsEffectType.StartInfrastructureDisruption,
                Service = InfrastructureServiceType.Transport,
                Severity = InfrastructureSeverity.Strained,
                DurationDays = 2
            }]
        };
        NewsRegistry.Configure([definition]);
        var news = new NewsState();
        var infrastructure = new InfrastructureState();
        var journal = new Slums.Core.State.EventJournal();
        NewsService.ResolveStartOfDay(news, infrastructure, journal, 2, new AlwaysGeneratingRandom());

        await Assert.That(news.ActiveFlashes).Count().IsGreaterThan(0);
        await Assert.That(infrastructure.Get(DistrictId.BulaqAlDakrour, InfrastructureServiceType.Transport).Severity).IsEqualTo(InfrastructureSeverity.Strained);
        await Assert.That(journal.Entries).Count().IsGreaterThan(0);
    }

    [Test]
    public async Task Inventory_ShouldRejectOverflowAndRemoveExactlyWhatWasUsed()
    {
        var inventory = new InventoryState();
        await Assert.That(inventory.Add("water_container", 2, 2)).IsTrue();
        await Assert.That(inventory.Add("water_container", 1, 2)).IsFalse();
        await Assert.That(inventory.Remove("water_container", 1)).IsTrue();
        await Assert.That(inventory.GetQuantity("water_container")).IsEqualTo(1);
        await Assert.That(inventory.Remove("water_container", 2)).IsFalse();
    }

    [Test]
    public async Task NpcAvailability_ShouldExposeTheReasonWhenNpcIsAtAnotherLocation()
    {
        var clock = new GameClock();
        clock.SetTime(1, 10, 0);
        var schedules = new[]
        {
            new NpcScheduleDefinition
            {
                Npc = NpcId.NeighborMona,
                Days = [GameDayOfWeek.Saturday],
                StartMinute = 360,
                EndMinute = 720,
                Location = LocationId.Laundry,
                AbsenceReason = "Mona is at the laundry until afternoon."
            }
        };

        var availability = NpcAvailabilityResolver.Resolve(NpcId.NeighborMona, clock, LocationId.Home, schedules);

        await Assert.That(availability.IsAvailable).IsFalse();
        await Assert.That(availability.Location).IsEqualTo(LocationId.Laundry);
        await Assert.That(availability.Reason).Contains("laundry");
    }

    private sealed class AlwaysGeneratingRandom : Random
    {
        public override int Next(int maxValue) => 0;
    }
}
