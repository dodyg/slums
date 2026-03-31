namespace Slums.Core.Economy;

public static class LoanSharkEscalation
{
    private const int StressPhase1 = 5;
    private const int StressPhase2 = 8;
    private const int HealthDamagePhase2 = 5;
    private const int ViolenceThresholdDays = 14;

    public static (int Stress, int Health, string Message) ApplyDailyPenalty(PlayerDebt debt, int currentDay)
    {
        ArgumentNullException.ThrowIfNull(debt);
        if (debt.Source != DebtSource.LoanShark)
        {
            return (0, 0, string.Empty);
        }

        var daysOverdue = debt.DaysOverdue(currentDay);
        if (daysOverdue <= 0)
        {
            return (0, 0, string.Empty);
        }

        if (daysOverdue <= 7)
        {
            return (StressPhase1, 0, "Someone reminds you: you owe money to dangerous people. The pressure builds.");
        }

        if (daysOverdue < ViolenceThresholdDays)
        {
            return (StressPhase2, -HealthDamagePhase2, "A loan shark's enforcer finds you. The message is physical.");
        }

        return (0, 0, string.Empty);
    }

    public static bool ShouldTriggerViolence(PlayerDebt debt, int currentDay)
    {
        ArgumentNullException.ThrowIfNull(debt);
        return debt.Source == DebtSource.LoanShark && debt.DaysOverdue(currentDay) >= ViolenceThresholdDays;
    }
}
