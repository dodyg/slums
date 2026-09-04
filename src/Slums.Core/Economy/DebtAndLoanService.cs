using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Economy;

/// <summary>Applies player debt, loan, rent, and lending actions through a session.</summary>
internal static class DebtAndLoanService
{
    internal static void ApplyRentPayment(GameSession session, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (amount <= 0 || session.AccumulatedRentDebt <= 0)
        {
            return;
        }

        var payment = Math.Min(Math.Min(amount, session.AccumulatedRentDebt), session.Player.Stats.Money);
        if (payment <= 0)
        {
            return;
        }

        session.Player.Stats.ModifyMoney(-payment);
        session.Rent.PayPartialDebt(payment);
        session.RaiseAutoTransaction($"Paid {payment} LE toward rent arrears.");
    }

    internal static void GrantRentGraceDays(GameSession session, int days)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Rent.AddGraceDays(days);
        if (days > 0)
        {
            session.RaiseEvent($"The landlord grants {days} day{(days == 1 ? string.Empty : "s")} of rent grace.");
        }
    }

    internal static void ApplyDebtPayment(GameSession session, DebtSource source, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = DebtService.Repay(
            source,
            Math.Min(amount, session.Player.Stats.Money),
            session.Player,
            session.PlayerDebts,
            session.Relationships,
            session.DistrictHeat,
            session.World.CurrentDistrict);
        if (!result.Success || result.Payment <= 0)
        {
            return;
        }

        if (result.FullyRepaid)
        {
            var creditorName = result.CreditorNpc?.ToString() ?? source.ToString();
            session.RaiseAutoTransaction($"Debt to {creditorName} fully repaid: {result.Payment} LE.");
        }
        else
        {
            session.RaiseAutoTransaction($"Repaid {result.Payment} LE toward {source} debt. Remaining: {result.Remaining} LE.");
        }
    }

    internal static void ExtendDebtDueDate(GameSession session, DebtSource source, int days)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.PlayerDebts.ExtendDueDate(source, days))
        {
            session.RaiseEvent($"The {source} due date moves back {days} day{(days == 1 ? string.Empty : "s")}.");
        }
    }

    internal static (bool Success, int Amount, string Message) BorrowFromNpc(GameSession session, NpcId npc, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = DebtService.BorrowFromNpc(npc, amount, session.Clock.Day, session.Player, session.Relationships, session.NpcEconomies, session.PlayerDebts);
        if (!result.Success)
        {
            return result;
        }

        var before = session.CaptureStats();
        var debt = session.PlayerDebts.Debts[^1];
        session.RecordMutation(MutationCategories.Economy, "TryBorrowFromNpc", before, session.CaptureStats(), $"Borrowed {result.Amount} LE from {npc} (due day {debt.DueDay})");
        session.RaiseAutoTransaction($"Borrowed {result.Amount} LE from {npc}.");
        return result;
    }

    internal static (bool Success, int Amount, string Message) BorrowFromLandlord(GameSession session, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = DebtService.BorrowFromLandlord(amount, session.Clock.Day, session.Player, session.Relationships, session.Rent, session.PlayerDebts);
        if (!result.Success)
        {
            return result;
        }

        var before = session.CaptureStats();
        session.RecordMutation(MutationCategories.Economy, "TryBorrowFromLandlord", before, session.CaptureStats(), $"Landlord advance: {result.Amount} LE (added to rent debt)");
        session.RaiseAutoTransaction($"Hajj Mahmoud advances you {result.Amount} LE. It's added to your rent debt.");
        return result;
    }

    internal static (bool Success, int Amount, string Message) BorrowFromLoanShark(GameSession session, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = DebtService.BorrowFromLoanShark(
            amount,
            session.Clock.Day,
            session.Player.BackgroundType,
            session.Player,
            session.PlayerDebts,
            session.DistrictHeat,
            session.World.CurrentDistrict,
            session.SharedRandom);
        if (!result.Success)
        {
            return (false, 0, result.Message);
        }

        var before = session.CaptureStats();
        session.RecordMutation(MutationCategories.Economy, "TryBorrowFromLoanShark", before, session.CaptureStats(), $"Loan shark: {result.Amount} LE at {result.InterestBasisPoints}bps, due day {session.Clock.Day + 7}");
        session.RaiseAutoTransaction($"A loan shark hands you {result.Amount} LE. The interest is brutal. Due in 7 days.");
        return (true, result.Amount, result.Message);
    }

    internal static (bool Success, string Message) LendToNpc(GameSession session, NpcId npc, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (amount <= 0)
        {
            return (false, "Invalid amount.");
        }

        if (session.Player.Stats.Money < amount)
        {
            return (false, "You can't afford that.");
        }

        session.Player.Stats.ModifyMoney(-amount);
        session.Relationships.ModifyNpcTrust(npc, 4);
        session.Relationships.RecordFavor(npc, session.Clock.Day, hasUnpaidDebt: true);
        session.Relationships.SetHelpedState(npc, true);
        session.NpcEconomies.AddDebt(DebtorId.Player, new DebtorId.NpcDebtor(npc), amount);

        var before = session.CaptureStats();
        session.RecordMutation(MutationCategories.Economy, "TryLendToNpc", before, session.CaptureStats(), $"Lent {amount} LE to {npc}");
        session.RaiseAutoTransaction($"You lend {amount} LE to {npc}.");
        return (true, $"You lend {npc} {amount} LE. They'll remember this.");
    }

    internal static (bool Success, string Message) RefuseNpcLoan(GameSession session, NpcId npc)
    {
        ArgumentNullException.ThrowIfNull(session);
#pragma warning disable CA5394
        var trustLoss = session.SharedRandom.Next(2, 6);
#pragma warning restore CA5394
        session.Relationships.ModifyNpcTrust(npc, -trustLoss);
        session.Relationships.RecordRefusal(npc, session.Clock.Day);
        var before = session.CaptureStats();
        session.RecordMutation(MutationCategories.Economy, "RefuseNpcLoan", before, session.CaptureStats(), $"Refused loan to {npc}, trust -{trustLoss}");
        return (true, $"{npc} asked for help. You said no. Trust -{trustLoss}.");
    }

    internal static (bool Success, int Remaining, string Message) RepayDebt(GameSession session, DebtSource source, int amount)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = DebtService.Repay(source, amount, session.Player, session.PlayerDebts, session.Relationships, session.DistrictHeat, session.World.CurrentDistrict);
        if (!result.Success)
        {
            return (false, result.Remaining, result.Message);
        }

        if (result.FullyRepaid)
        {
            var creditorName = result.CreditorNpc?.ToString() ?? source.ToString();
            session.RaiseAutoTransaction($"Debt to {creditorName} fully repaid: {result.Payment} LE.");
        }
        else
        {
            session.RaiseAutoTransaction($"Repaid {result.Payment} LE toward {source} debt. Remaining: {result.Remaining} LE.");
        }

        var before = session.CaptureStats();
        session.RecordMutation(MutationCategories.Economy, "RepayDebt", before, session.CaptureStats(), $"Repaid {result.Payment} LE ({source}), remaining {result.Remaining} LE");
        return (true, result.Remaining, result.Message);
    }

    internal static void RestoreEconomyState(
        GameSession session,
        IEnumerable<(NpcId Npc, NpcWealthLevel WealthLevel, int Generosity,
            Dictionary<DebtorId, int> OwedTo, Dictionary<DebtorId, int> OwedBy,
            int LastHardshipDay, int LastWindfallDay, int GenerousUntilDay)> npcEconomies,
        IEnumerable<PlayerDebt> playerDebts)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(npcEconomies);
        ArgumentNullException.ThrowIfNull(playerDebts);
        foreach (var entry in npcEconomies)
        {
            session.NpcEconomies.RestoreEntry(entry.Npc, entry.WealthLevel, entry.Generosity, entry.OwedTo, entry.OwedBy, entry.LastHardshipDay, entry.LastWindfallDay, entry.GenerousUntilDay);
        }

        session.PlayerDebts.RestoreDebts(playerDebts);
    }
}
