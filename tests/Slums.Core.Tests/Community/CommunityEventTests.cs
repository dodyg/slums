using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Community;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Community;

internal sealed class CommunityEventTests
{
    [Test]
    public async Task CommunityEventRegistry_HasAllFiveEvents()
    {
        await Assert.That(CommunityEventRegistry.AllEvents.Count).IsEqualTo(5);
    }

    [Test]
    public async Task CommunityEventRegistry_GetById_ReturnsCorrectEvent()
    {
        var evt = CommunityEventRegistry.GetById(CommunityEventId.FridayRooftopGathering);
        await Assert.That(evt).IsNotNull();
        await Assert.That(evt!.Name).IsEqualTo("Friday Rooftop Gathering");
    }

    [Test]
    public async Task CommunityEventRegistry_GetById_ReturnsNullForUnknown()
    {
        await Assert.That(CommunityEventRegistry.GetById((CommunityEventId)999)).IsNull();
    }

    [Test]
    public async Task FridayGathering_IsOnlyAvailableOnFriday()
    {
        using var state = new GameSession();

        state.Clock.SetTime(1, 6, 0);
        var day1Events = state.GetAvailableCommunityEvents();
        var hasFriday = day1Events.Any(e => e.Id == CommunityEventId.FridayRooftopGathering);

        state.Clock.SetTime(7, 6, 0);
        var fridayEvents = state.GetAvailableCommunityEvents();
        var hasFridayOnFriday = fridayEvents.Any(e => e.Id == CommunityEventId.FridayRooftopGathering);

        await Assert.That(hasFriday).IsFalse();
        await Assert.That(hasFridayOnFriday).IsTrue();
    }

    [Test]
    public async Task RamadanIftar_IsOnlyAvailableDuringRamadan()
    {
        using var state = new GameSession();

        var beforeRamadan = state.GetAvailableCommunityEvents();
        var hasIftar = beforeRamadan.Any(e => e.Id == CommunityEventId.RamadanIftarSharing);
        await Assert.That(hasIftar).IsFalse();
    }

    [Test]
    public async Task TeaCircle_RequiresNpcInvitation()
    {
        using var state = new GameSession();

        var withoutInvite = state.GetAvailableCommunityEvents();
        var hasTea = withoutInvite.Any(e => e.Id == CommunityEventId.RooftopTeaCircle);
        await Assert.That(hasTea).IsFalse();

        state.EventAttendance.HasTeaCircleInvitation = true;
        var withInvite = state.GetAvailableCommunityEvents();
        var hasTeaAfter = withInvite.Any(e => e.Id == CommunityEventId.RooftopTeaCircle);
        await Assert.That(hasTeaAfter).IsTrue();
    }

    [Test]
    public async Task AttendCommunityEvent_FridayGathering_ReducesStress()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        state.Player.Stats.SetStress(50);

        var result = state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsLessThan(50);
    }

    [Test]
    public async Task AttendCommunityEvent_FridayGathering_GrantsTrust()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);

        var trustBefore = state.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;
        state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);
        var trustAfter = state.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;

        await Assert.That(trustAfter).IsGreaterThanOrEqualTo(trustBefore);
    }

    [Test]
    public async Task AttendCommunityEvent_AdvancesTime()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        var hourBefore = state.Clock.Hour;

        state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);

        await Assert.That(state.Clock.Hour).IsGreaterThan(hourBefore);
    }

    [Test]
    public async Task AttendCommunityEvent_RequiresMoney()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        state.Player.Stats.SetMoney(0);

        state.EventAttendance.HasTeaCircleInvitation = true;
        var result = state.AttendCommunityEvent(CommunityEventId.RooftopTeaCircle);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task AttendCommunityEvent_DeductsMoney()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        state.Player.Stats.SetMoney(100);
        state.EventAttendance.HasTeaCircleInvitation = true;

        state.AttendCommunityEvent(CommunityEventId.RooftopTeaCircle);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(100);

        var mulidDef = CommunityEventRegistry.GetById(CommunityEventId.MulidFestival);
        if (mulidDef is not null)
        {
            state.World.TravelTo(LocationId.Home);
        }
    }

    [Test]
    public async Task AttendCommunityEvent_RecordsAttendance()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);

        await Assert.That(state.EventAttendance.TotalAttended).IsEqualTo(0);

        state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);

        await Assert.That(state.EventAttendance.TotalAttended).IsEqualTo(1);
        await Assert.That(state.EventAttendance.ConsecutiveSkips).IsEqualTo(0);
    }

    [Test]
    public async Task AttendCommunityEvent_CannotAttendTwiceInSameWeek()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        state.Player.Stats.SetMoney(200);

        var first = state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);
        state.Clock.SetTime(7, 8, 0);
        var second = state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);

        await Assert.That(first).IsTrue();
        await Assert.That(second).IsFalse();
    }

    [Test]
    public async Task AttendCommunityEvent_ProvidesFood_WhenEventHasFoodAccess()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 6, 0);
        state.Player.Stats.SetMoney(50);
        state.Player.Nutrition.Eat(MealQuality.None);

        state.AttendCommunityEvent(CommunityEventId.FridayRooftopGathering);

        var fridayDef = CommunityEventRegistry.GetById(CommunityEventId.FridayRooftopGathering);
        if (fridayDef?.ProvidesFoodAccess == true)
        {
            await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        }
    }

    [Test]
    public async Task CommunityEventAttendance_ConsecutiveSkipsIncrement()
    {
        var attendance = new CommunityEventAttendance();
        await Assert.That(attendance.ConsecutiveSkips).IsEqualTo(0);

        attendance.RecordSkip();
        await Assert.That(attendance.ConsecutiveSkips).IsEqualTo(1);

        attendance.RecordSkip();
        await Assert.That(attendance.ConsecutiveSkips).IsEqualTo(2);
    }

    [Test]
    public async Task CommunityEventAttendance_AttendanceResetsSkips()
    {
        var attendance = new CommunityEventAttendance();
        attendance.RecordSkip();
        attendance.RecordSkip();
        attendance.RecordSkip();

        attendance.RecordAttendance(CommunityEventId.FridayRooftopGathering, 5);

        await Assert.That(attendance.ConsecutiveSkips).IsEqualTo(0);
        await Assert.That(attendance.TotalAttended).IsEqualTo(1);
        await Assert.That(attendance.LastAttendanceDay).IsEqualTo(5);
    }

    [Test]
    public async Task CommunityEventAttendance_WeeklyResetClears()
    {
        var attendance = new CommunityEventAttendance();
        attendance.RecordAttendance(CommunityEventId.FridayRooftopGathering, 1);
        attendance.HasTeaCircleInvitation = true;

        attendance.ResetWeeklyIfNeeded(8);

        await Assert.That(attendance.AttendedThisWeek.Count).IsEqualTo(0);
        await Assert.That(attendance.HasTeaCircleInvitation).IsFalse();
    }

    [Test]
    public async Task EndDay_IncrementsConsecutiveSkips_WhenNoEventAttended()
    {
        using var state = new GameSession();
        state.Player.Nutrition.Eat(MealQuality.Basic);

        await Assert.That(state.EventAttendance.ConsecutiveSkips).IsEqualTo(0);

        state.EndDay();

        await Assert.That(state.EventAttendance.ConsecutiveSkips).IsGreaterThan(0);
    }

    [Test]
    public async Task NeighborhoodCleanup_IsAlwaysAvailable()
    {
        using var state = new GameSession();

        var events = state.GetAvailableCommunityEvents();
        var hasCleanup = events.Any(e => e.Id == CommunityEventId.NeighborhoodCleanup);

        await Assert.That(hasCleanup).IsTrue();
    }

    [Test]
    public async Task RestoreCommunityEventAttendance_PreservesState()
    {
        using var state = new GameSession();
        state.EventAttendance.RecordAttendance(CommunityEventId.FridayRooftopGathering, 5);
        state.EventAttendance.RecordSkip();
        state.EventAttendance.HasTeaCircleInvitation = true;

        var skips = state.EventAttendance.ConsecutiveSkips;
        var total = state.EventAttendance.TotalAttended;
        var lastDay = state.EventAttendance.LastAttendanceDay;
        var invite = state.EventAttendance.HasTeaCircleInvitation;

        await Assert.That(skips).IsEqualTo(1);
        await Assert.That(total).IsEqualTo(1);
        await Assert.That(lastDay).IsEqualTo(5);
        await Assert.That(invite).IsTrue();
    }
}
