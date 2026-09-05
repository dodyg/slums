using Slums.Core.Characters;
using Slums.Core.Expenses;
using Slums.Core.Heat;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.Economy;

/// <summary>
/// Owns player debt rules and applies debt mutations to the supplied domain state.
/// </summary>
public sealed class DebtService
{
    public const int NpcLoanMinimumTrust = 10;
    public const int LandlordLoanMinimumTrust = 5;
    public const int StandardNpcLoanCap = 30;
    public const int GenerousNpcLoanCap = 50;
    public const int LandlordLoanMinimum = 50;
    public const int LandlordLoanCap = 100;
    public const int LoanSharkMinimum = 100;
    public const int LoanSharkCap = 300;
    public const int ReleasedPrisonerLoanSharkCap = 200;
    public const int NpcLoanDueDays = 14;
    public const int StrugglingNpcLoanDueDays = 7;
    public const int MedicalBackgroundLoanDueDays = 21;
    public const int LandlordLoanDueDays = 14;
    public const int LoanSharkDueDays = 7;
    public const int LoanSharkHeat = 5;
    public const int RepaidCreditorTrust = 3;
    public const int RepaidLoanSharkHeatReduction = 3;

    /// <summary>Attempts to create a standard NPC or community loan.</summary>
    public static (bool Success, int Amount, string Message) BorrowFromNpc(
        NpcId npc,
        int amount,
        int currentDay,
        PlayerCharacter player,
        RelationshipState relationships,
        NpcEconomyState npcEconomies,
        PlayerDebtState playerDebts)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(npcEconomies);
        ArgumentNullException.ThrowIfNull(playerDebts);

        if (amount <= 0)
        {
            return (false, 0, "Invalid amount.");
        }

        var relationship = relationships.GetNpcRelationship(npc);
        if (relationship.Trust < NpcLoanMinimumTrust)
        {
            return (false, 0, $"{npc} doesn't trust you enough for a loan.");
        }

        if (relationship.HasUnpaidDebt)
        {
            return (false, 0, $"You already owe {npc}.");
        }

        var economy = npcEconomies.GetEconomy(npc);
        if (economy.WealthLevel == NpcWealthLevel.Struggling)
        {
            return (false, 0, $"{npc} can't afford to lend right now.");
        }

        var maxAmount = economy.Generosity >= 7 ? GenerousNpcLoanCap : StandardNpcLoanCap;
        var actualAmount = Math.Min(amount, maxAmount);
        var source = npc == NpcId.LandlordHajjMahmoud ? DebtSource.LandlordAdvance : DebtSource.NeighborLoan;
        if (player.BackgroundType == BackgroundType.SudaneseRefugee && source == DebtSource.NeighborLoan)
        {
            source = DebtSource.CommunityMutualAid;
        }

        if (source == DebtSource.LandlordAdvance)
        {
            actualAmount = Math.Min(amount, LandlordLoanCap);
        }

        var dueDay = currentDay + NpcLoanDueDays;
        if (source == DebtSource.NeighborLoan && economy.Generosity < 5)
        {
            dueDay = currentDay + StrugglingNpcLoanDueDays;
        }

        if (player.BackgroundType == BackgroundType.MedicalSchoolDropout && economy.Generosity >= 5)
        {
            dueDay = currentDay + MedicalBackgroundLoanDueDays;
        }

        player.Stats.ModifyMoney(actualAmount);
        relationships.SetDebtState(npc, true);
        playerDebts.AddDebt(new PlayerDebt
        {
            Source = source,
            AmountOwed = actualAmount,
            InterestWeeklyBasisPoints = 0,
            DueDay = dueDay,
            CollectionState = DebtCollectionState.Current,
            OriginDay = currentDay,
            CreditorNpcId = (int)npc
        });

        return (true, actualAmount, $"{npc} lends you {actualAmount} LE. Pay it back by day {dueDay}.");
    }

    /// <summary>Attempts to add a landlord advance to the rent account.</summary>
    public static (bool Success, int Amount, string Message) BorrowFromLandlord(
        int amount,
        int currentDay,
        PlayerCharacter player,
        RelationshipState relationships,
        RentState rentState,
        PlayerDebtState playerDebts)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(rentState);
        ArgumentNullException.ThrowIfNull(playerDebts);

        if (amount <= 0)
        {
            return (false, 0, "Invalid amount.");
        }

        var creditor = relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud);
        if (creditor.Trust < LandlordLoanMinimumTrust)
        {
            return (false, 0, "Hajj Mahmoud won't advance you anything.");
        }

        if (creditor.HasUnpaidDebt)
        {
            return (false, 0, "You already owe the landlord.");
        }

        var actualAmount = Math.Clamp(amount, LandlordLoanMinimum, LandlordLoanCap);
        player.Stats.ModifyMoney(actualAmount);
        relationships.SetDebtState(NpcId.LandlordHajjMahmoud, true);
        rentState.PayPartialDebt(-actualAmount);
        playerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LandlordAdvance,
            AmountOwed = actualAmount,
            InterestWeeklyBasisPoints = 0,
            DueDay = currentDay + LandlordLoanDueDays,
            CollectionState = DebtCollectionState.Current,
            OriginDay = currentDay,
            CreditorNpcId = (int)NpcId.LandlordHajjMahmoud
        });

        return (true, actualAmount, $"Hajj Mahmoud advances {actualAmount} LE. It goes on your rent account.");
    }

    /// <summary>Attempts to create a loan-shark debt and increase current-district heat.</summary>
    public static (bool Success, int Amount, int InterestBasisPoints, string Message) BorrowFromLoanShark(
        int amount,
        int currentDay,
        BackgroundType backgroundType,
        PlayerCharacter player,
        PlayerDebtState playerDebts,
        DistrictHeatState districtHeat,
        DistrictId currentDistrict,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(playerDebts);
        ArgumentNullException.ThrowIfNull(districtHeat);
        ArgumentNullException.ThrowIfNull(random);

        if (amount <= 0)
        {
            return (false, 0, 0, "Invalid amount.");
        }

        if (playerDebts.Debts.Any(static debt => debt.Source == DebtSource.LoanShark))
        {
            return (false, 0, 0, "You already have an outstanding loan shark debt. Settle it first.");
        }

        var maxAmount = backgroundType == BackgroundType.ReleasedPoliticalPrisoner
            ? ReleasedPrisonerLoanSharkCap
            : LoanSharkCap;
        var actualAmount = Math.Clamp(amount, LoanSharkMinimum, maxAmount);
#pragma warning disable CA5394
        var interestBps = random.Next(2000, 3000);
#pragma warning restore CA5394

        player.Stats.ModifyMoney(actualAmount);
        districtHeat.AddHeat(currentDistrict, LoanSharkHeat);
        playerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = actualAmount,
            InterestWeeklyBasisPoints = interestBps,
            DueDay = currentDay + LoanSharkDueDays,
            CollectionState = DebtCollectionState.Current,
            OriginDay = currentDay
        });

        return (true, actualAmount, interestBps, $"You take {actualAmount} LE from a loan shark. Interest compounds weekly. Due in 7 days. Police pressure rises.");
    }

    /// <summary>Repays a debt from available money and restores the creditor relationship when complete.</summary>
    public static (bool Success, int Remaining, int Payment, bool FullyRepaid, NpcId? CreditorNpc, string Message) Repay(
        DebtSource source,
        int amount,
        PlayerCharacter player,
        PlayerDebtState playerDebts,
        RelationshipState relationships,
        DistrictHeatState districtHeat,
        DistrictId currentDistrict)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(playerDebts);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(districtHeat);

        if (amount <= 0)
        {
            return (false, 0, 0, false, null, "Invalid amount.");
        }

        var debt = playerDebts.Debts.FirstOrDefault(candidate => candidate.Source == source);
        if (debt is null)
        {
            return (false, 0, 0, false, null, "No such debt to repay.");
        }

        var payment = Math.Min(amount, debt.AmountOwed);
        if (player.Stats.Money < payment)
        {
            return (false, debt.AmountOwed, 0, false, null, "Not enough money.");
        }

        player.Stats.ModifyMoney(-payment);
        playerDebts.RepayPartial(source, payment);
        var remaining = playerDebts.Debts.FirstOrDefault(candidate => candidate.Source == source)?.AmountOwed ?? 0;
        var fullyRepaid = remaining <= 0;
        NpcId? creditorNpc = null;
        if (fullyRepaid && debt.CreditorNpcId.HasValue && Enum.IsDefined(typeof(NpcId), debt.CreditorNpcId.Value))
        {
            var resolvedCreditorNpc = (NpcId)debt.CreditorNpcId.Value;
            creditorNpc = resolvedCreditorNpc;
            relationships.SetDebtState(resolvedCreditorNpc, false);
            relationships.ModifyNpcTrust(resolvedCreditorNpc, RepaidCreditorTrust);
        }

        if (source == DebtSource.LoanShark && fullyRepaid)
        {
            districtHeat.AddHeat(currentDistrict, -RepaidLoanSharkHeatReduction);
        }

        return (true, remaining, payment, fullyRepaid, creditorNpc, fullyRepaid
            ? $"{source} debt fully repaid!"
            : $"Paid {payment} LE. {remaining} LE remaining on {source} debt.");
    }

    /// <summary>Applies the daily loan-shark penalty and reports whether collection ends the run.</summary>
    public static DebtEscalationResult ProcessDailyLoanShark(PlayerDebtState playerDebts, SurvivalStats stats, int currentDay, int composureSkillLevel = 0)
    {
        ArgumentNullException.ThrowIfNull(playerDebts);
        ArgumentNullException.ThrowIfNull(stats);

        foreach (var debt in playerDebts.GetOverdueDebts(currentDay))
        {
            if (debt.Source != DebtSource.LoanShark)
            {
                continue;
            }

            var penalty = LoanSharkEscalation.ApplyDailyPenalty(debt, currentDay);
            stats.ModifyStress(ComposureCalculator.GetDebtStressCost(composureSkillLevel, penalty.Stress));
            stats.ModifyHealth(penalty.Health);
            return new DebtEscalationResult(penalty.Message, LoanSharkEscalation.ShouldTriggerViolence(debt, currentDay));
        }

        return DebtEscalationResult.None;
    }
}

/// <summary>Result of processing a daily loan-shark collection attempt.</summary>
public sealed record DebtEscalationResult(string Message, bool TriggersDestitution)
{
    public static DebtEscalationResult None { get; } = new(string.Empty, false);
}
