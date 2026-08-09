using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit;

namespace Slums.Infrastructure.Tests;

internal sealed class RandomnessSaveLoadTests
{
    [Test]
    public async Task RestoredSession_ContinuesIdenticalOutcomes()
    {
        var original = CreateSeededSession();

        // Original run: three days, then save.
        EndDays(original, 3);
        var save = GameSessionSnapshot.Capture(original);
        var originalAtSave = CaptureComparableState(original);

        // Restored session must match the original exactly at the save point.
        var restored = save.Restore();
        var restoredAtSave = CaptureComparableState(restored);

        var savePointDifferences = originalAtSave
            .Zip(restoredAtSave)
            .Select((pair, index) => pair.First == pair.Second ? null : $"line {index}: original={pair.First} restored={pair.Second}")
            .Where(static difference => difference is not null)
            .ToArray();

        savePointDifferences.Should().BeEmpty("the restore must reproduce the saved state exactly");

        // Original run continues two more days from the save point.
        EndDays(original, 2);
        var originalFinal = CaptureComparableState(original);

        // Restored run replays the same actions from the save.
        EndDays(restored, 2);
        var restoredFinal = CaptureComparableState(restored);

        var differences = originalFinal
            .Zip(restoredFinal)
            .Select((pair, index) => pair.First == pair.Second ? null : $"line {index}: original={pair.First} restored={pair.Second}")
            .Where(static difference => difference is not null)
            .ToArray();

        differences.Should().BeEmpty("the restored session must reproduce the uninterrupted run's outcomes exactly");
    }

    [Test]
    public async Task Capture_IncludesRandomState_ForGameRandomBackedSession()
    {
        var session = CreateSeededSession();
        session.EndDay();
        session.EndDay();

        var save = GameSessionSnapshot.Capture(session);

        save.RandomState.Should().NotBeNull("a GameRandom-backed session must persist its random state");
    }

    [Test]
    public async Task RestoredSession_RandomStream_MatchesOriginalAtSavePoint()
    {
        var original = CreateSeededSession();
        EndDays(original, 2);
        var save = GameSessionSnapshot.Capture(original);

        // Draws from the original after the save point.
#pragma warning disable CA5394 // Gameplay randomness does not require cryptographic strength
        var originalDraws = Enumerable.Range(0, 50).Select(_ => original.SharedRandom.Next(1000)).ToArray();
#pragma warning restore CA5394

        var restored = save.Restore();
#pragma warning disable CA5394 // Gameplay randomness does not require cryptographic strength
        var restoredDraws = Enumerable.Range(0, 50).Select(_ => restored.SharedRandom.Next(1000)).ToArray();
#pragma warning restore CA5394

        restoredDraws.Should().Equal(originalDraws, "the restored random stream must continue where the original left off");
    }

    [Test]
    public async Task RestoredSession_PreservesDistrictConditionsRolledAtConstruction()
    {
        var original = CreateSeededSession();
        var originalConditions = original.World.ActiveDistrictConditions.Select(static c => c.District.ToString()).OrderBy(static d => d).ToArray();

        var save = GameSessionSnapshot.Capture(original);
        var restored = save.Restore();
        var restoredConditions = restored.World.ActiveDistrictConditions.Select(static c => c.District.ToString()).OrderBy(static d => d).ToArray();

        restoredConditions.Should().Equal(originalConditions);
    }

    private static GameSession CreateSeededSession()
    {
        var session = new GameSession(new GameRandom(20260809));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        return session;
    }

    private static void EndDays(GameSession session, int days)
    {
        for (var i = 0; i < days; i++)
        {
            session.EndDay();
        }
    }

    private static string[] CaptureComparableState(GameSession session)
    {
        var state = new List<string>
        {
            $"day={session.Clock.Day}",
            $"money={session.Player.Stats.Money}",
            $"energy={session.Player.Stats.Energy}",
            $"health={session.Player.Stats.Health}",
            $"stress={session.Player.Stats.Stress}",
            $"satiety={session.Player.Nutrition.Satiety}",
            $"motherHealth={session.Player.Household.MotherHealth}",
            $"food={session.Player.Household.FoodStockpile}",
            $"weather={session.CurrentWeather.Type}",
            $"pressure={session.PolicePressure}",
            $"unpaidRent={session.UnpaidRentDays}",
            $"rentDebt={session.AccumulatedRentDebt}",
            $"crimeEarnings={session.TotalCrimeEarnings}",
            $"crimes={session.CrimesCommitted}",
            $"workEarnings={session.TotalHonestWorkEarnings}",
            $"shifts={session.HonestShiftsCompleted}",
            $"investments={session.TotalInvestmentEarnings}",
            $"daysSurvived={session.DaysSurvived}",
            $"pendingEnding={session.PendingEndingKnot ?? "<none>"}"
        };

        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            state.Add($"trust:{npcId}={session.Relationships.GetNpcRelationship(npcId).Trust}");
        }

        foreach (var factionId in Enum.GetValues<FactionId>())
        {
            state.Add($"faction:{factionId}={session.Relationships.GetFactionStanding(factionId).Reputation}");
        }

        foreach (var pair in session.RandomEventHistory.OrderBy(static p => p.Key))
        {
            state.Add($"event:{pair.Key}={pair.Value}");
        }

        foreach (var flag in session.StoryFlags.OrderBy(static f => f))
        {
            state.Add($"flag:{flag}");
        }

        return state.ToArray();
    }
}
