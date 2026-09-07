using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed class GameSessionEconomySnapshot
{
    public IReadOnlyList<NpcEconomyEntrySnapshot> NpcEconomies { get; init; } = [];
    public IReadOnlyList<PlayerDebtEntrySnapshot> PlayerDebts { get; init; } = [];

    public static GameSessionEconomySnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionEconomySnapshot
        {
            NpcEconomies = gameSession.NpcEconomies.Economies.Values
                .Select(static e => new NpcEconomyEntrySnapshot
                {
                    Npc = e.Npc.ToString(),
                    WealthLevel = e.WealthLevel.ToString(),
                    Generosity = e.Generosity,
                    MoneyOwedTo = e.MoneyOwedTo.Select(ToSnapshot).ToArray(),
                    MoneyOwedBy = e.MoneyOwedBy.Select(ToSnapshot).ToArray(),
                    LastHardshipDay = e.LastHardshipDay,
                    LastWindfallDay = e.LastWindfallDay,
                    GenerousUntilDay = e.GenerousUntilDay
                })
                .ToArray(),
            PlayerDebts = gameSession.PlayerDebts.Debts
                .Select(static d => new PlayerDebtEntrySnapshot
                {
                    Source = d.Source.ToString(),
                    AmountOwed = d.AmountOwed,
                    InterestWeeklyBasisPoints = d.InterestWeeklyBasisPoints,
                    DueDay = d.DueDay,
                    CollectionState = d.CollectionState.ToString(),
                    OriginDay = d.OriginDay,
                    CreditorNpcId = d.CreditorNpcId
                })
                .ToArray()
        };
    }

    private static DebtorAmountSnapshot ToSnapshot(KeyValuePair<DebtorId, int> kvp)
    {
        var npcId = kvp.Key.TryGetNpcId();
        return new DebtorAmountSnapshot
        {
            DebtorType = kvp.Key.IsNpc ? "Npc" : "Player",
            NpcId = npcId?.ToString(),
            Amount = kvp.Value
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var npcEconomies = new List<(NpcId, NpcWealthLevel, int, Dictionary<DebtorId, int>, Dictionary<DebtorId, int>, int, int, int)>();

        foreach (var entry in NpcEconomies)
        {
            var npc = SaveValueParser.ParseEnum<NpcId>(entry.Npc, "NPC economy NPC");
            var wealth = SaveValueParser.ParseEnum<NpcWealthLevel>(entry.WealthLevel, "NPC wealth level");

            var owedTo = RestoreDebtorMap(entry.MoneyOwedTo);
            var owedBy = RestoreDebtorMap(entry.MoneyOwedBy);

            npcEconomies.Add((npc, wealth, entry.Generosity, owedTo, owedBy, entry.LastHardshipDay, entry.LastWindfallDay, entry.GenerousUntilDay));
        }

        var playerDebts = new List<PlayerDebt>();
        foreach (var d in PlayerDebts)
        {
            var source = SaveValueParser.ParseEnum<DebtSource>(d.Source, "debt source");
            var state = SaveValueParser.ParseEnum<DebtCollectionState>(d.CollectionState, "debt collection state");

            playerDebts.Add(new PlayerDebt
            {
                Source = source,
                AmountOwed = d.AmountOwed,
                InterestWeeklyBasisPoints = d.InterestWeeklyBasisPoints,
                DueDay = d.DueDay,
                CollectionState = state,
                OriginDay = d.OriginDay,
                CreditorNpcId = d.CreditorNpcId
            });
        }

        gameSession.RestoreEconomyState(
            npcEconomies.Select(e => (e.Item1, e.Item2, e.Item3, e.Item4, e.Item5, e.Item6, e.Item7, e.Item8)),
            playerDebts);
    }

    private static Dictionary<DebtorId, int> RestoreDebtorMap(IReadOnlyList<DebtorAmountSnapshot> snapshots)
    {
        var result = new Dictionary<DebtorId, int>();
        foreach (var snap in snapshots)
        {
            DebtorId debtor;
            if (snap.DebtorType == "Npc" && snap.NpcId is not null)
            {
                debtor = DebtorId.FromNpc(SaveValueParser.ParseEnum<NpcId>(snap.NpcId, "debtor NPC"));
            }
            else
            {
                debtor = snap.DebtorType == "Player"
                    ? DebtorId.Player
                    : throw new InvalidDataException($"Save field 'debtor type' contains unknown value '{snap.DebtorType}'.");
            }

            result[debtor] = snap.Amount;
        }
        return result;
    }
}
