using Slums.Core.Characters;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

#pragma warning disable CA1001 // Type owns disposable field but transfers GameSession ownership via Build()

namespace Slums.Narrative.Ink.Tests.Helpers;

internal sealed class GameStateBuilder
{
    private readonly GameSession _session = new();

    public GameStateBuilder WithBackground(BackgroundType backgroundType)
    {
        var background = backgroundType switch
        {
            BackgroundType.MedicalSchoolDropout => BackgroundRegistry.MedicalSchoolDropout,
            BackgroundType.ReleasedPoliticalPrisoner => BackgroundRegistry.ReleasedPoliticalPrisoner,
            BackgroundType.SudaneseRefugee => BackgroundRegistry.SudaneseRefugee,
            _ => throw new ArgumentOutOfRangeException(nameof(backgroundType))
        };

        _session.Player.ApplyBackground(background);
        return this;
    }

    public GameStateBuilder WithMoney(int money)
    {
        _session.Player.Stats.SetMoney(money);
        return this;
    }

    public GameStateBuilder WithHealth(int health)
    {
        _session.Player.Stats.SetHealth(health);
        return this;
    }

    public GameStateBuilder WithEnergy(int energy)
    {
        _session.Player.Stats.SetEnergy(energy);
        return this;
    }

    public GameStateBuilder WithHunger(int hunger)
    {
        _session.Player.Stats.SetHunger(hunger);
        return this;
    }

    public GameStateBuilder WithStress(int stress)
    {
        _session.Player.Stats.SetStress(stress);
        return this;
    }

    public GameStateBuilder WithMotherHealth(int health)
    {
        _session.Player.Household.SetMotherHealth(health);
        return this;
    }

    public GameStateBuilder WithFoodStockpile(int stockpile)
    {
        _session.Player.Household.SetFoodStockpile(stockpile);
        return this;
    }

    public GameStateBuilder WithPolicePressure(int pressure)
    {
        _session.SetPolicePressure(pressure);
        return this;
    }

    public GameStateBuilder WithDaysSurvived(int days)
    {
        for (var i = 0; i < days; i++)
        {
            _session.SetDaysSurvived(_session.DaysSurvived + 1);
        }
        return this;
    }

    public GameStateBuilder WithCrimeCounters(int totalEarnings, int crimesCommitted, int lastCrimeDay = 0)
    {
        _session.SetCrimeCounters(totalEarnings, crimesCommitted, lastCrimeDay);
        return this;
    }

    public GameStateBuilder WithWorkCounters(int totalEarnings, int shiftsCompleted, int lastWorkDay = 0, int lastPublicWorkDay = 0)
    {
        _session.SetWorkCounters(totalEarnings, shiftsCompleted, lastWorkDay, lastPublicWorkDay);
        return this;
    }

    public GameStateBuilder WithNpcTrust(NpcId npcId, int trust, int lastSeenDay = 0)
    {
        _session.Relationships.SetNpcRelationship(npcId, trust, lastSeenDay);
        return this;
    }

    public GameStateBuilder WithFactionReputation(FactionId factionId, int reputation)
    {
        _session.Relationships.SetFactionStanding(factionId, reputation);
        return this;
    }

    public GameStateBuilder WithStoryFlag(string flag)
    {
        _session.SetStoryFlag(flag);
        return this;
    }

    public GameStateBuilder AtLocation(LocationId locationId)
    {
        _session.World.TravelTo(locationId);
        return this;
    }

    public GameStateBuilder OnDay(int day)
    {
        _session.Clock.SetTime(day, _session.Clock.Hour, _session.Clock.Minute);
        return this;
    }

    public GameSession Build()
    {
        return _session;
    }

    public static GameSession BuildForEnding(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.MotherDied => new GameStateBuilder()
                .WithMotherHealth(0)
                .Build(),

            EndingId.Destitution => new GameStateBuilder()
                .WithMoney(0)
                .WithHunger(10)
                .WithEnergy(5)
                .Build(),

            EndingId.Arrested => new GameStateBuilder()
                .WithPolicePressure(100)
                .Build(),

            EndingId.Eviction => new GameStateBuilder()
                .Build(),

            EndingId.NetworkShelter => new GameStateBuilder()
                .WithDaysSurvived(30)
                .WithMoney(150)
                .WithNpcTrust(NpcId.NeighborMona, 40)
                .WithNpcTrust(NpcId.NurseSalma, 40)
                .WithNpcTrust(NpcId.CafeOwnerNadia, 35)
                .WithNpcTrust(NpcId.FenceHanan, 35)
                .Build(),

            EndingId.QuitTheLuxorDream => new GameStateBuilder()
                .WithDaysSurvived(30)
                .WithMoney(550)
                .WithCrimeCounters(0, 2)
                .WithMotherHealth(70)
                .Build(),

            EndingId.StabilityHonestWork => new GameStateBuilder()
                .WithDaysSurvived(30)
                .WithMoney(250)
                .WithPolicePressure(10)
                .WithWorkCounters(400, 15, 30, 30)
                .Build(),

            EndingId.CrimeKingpin => new GameStateBuilder()
                .WithCrimeCounters(1100, 20)
                .WithFactionReputation(FactionId.ImbabaCrew, 55)
                .Build(),

            _ => throw new ArgumentOutOfRangeException(nameof(endingId))
        };
    }

    public static GameSession BuildForBackground(BackgroundType backgroundType)
    {
        return new GameStateBuilder()
            .WithBackground(backgroundType)
            .Build();
    }
}

#pragma warning restore CA1001
