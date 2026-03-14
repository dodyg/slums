namespace Slums.Core.Investments;

public static class InvestmentResolutionCalculator
{
#pragma warning disable CA5394 // Random is sufficient for gameplay mechanics
    public static InvestmentResolutionCalculation Resolve(Investment investment, InvestmentDefinition? definition, int currentMoney, Random rng)
    {
        ArgumentNullException.ThrowIfNull(investment);
        ArgumentNullException.ThrowIfNull(rng);

        if (definition is null)
        {
            return new InvestmentResolutionCalculation(
                new InvestmentResolution(investment.Type, 0, false, 0, 0, 0, "Investment definition not found."),
                ShouldSuspend: false);
        }

        var profile = investment.RiskProfile;

        if (rng.NextDouble() < profile.WeeklyFailureChance)
        {
            return new InvestmentResolutionCalculation(
                new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: true,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 0,
                    InvestedAmountLost: investment.InvestedAmount,
                    $"Your {definition.Name} venture failed. Investment lost."),
                ShouldSuspend: false);
        }

        if (rng.NextDouble() < profile.ExtortionChance)
        {
            var extortionAmount = rng.Next(profile.ExtortionAmountMin, profile.ExtortionAmountMax + 1);

            if (currentMoney >= extortionAmount)
            {
                return new InvestmentResolutionCalculation(
                    new InvestmentResolution(
                        investment.Type,
                        0,
                        WasLost: false,
                        ExtortionPaid: extortionAmount,
                        PolicePressureIncrease: 0,
                        InvestedAmountLost: 0,
                        $"Gangs demanded {extortionAmount} LE from your {definition.Name} operation."),
                    ShouldSuspend: false);
            }

            return new InvestmentResolutionCalculation(
                new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 2,
                    InvestedAmountLost: 0,
                    $"Could not pay extortion for {definition.Name}. Operation suspended."),
                ShouldSuspend: true);
        }

        if (rng.NextDouble() < profile.PoliceHeatChance)
        {
            return new InvestmentResolutionCalculation(
                new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 5,
                    InvestedAmountLost: 0,
                    $"Police interest in your {definition.Name} increases pressure."),
                ShouldSuspend: false);
        }

        if (rng.NextDouble() < profile.BetrayalChance)
        {
            return new InvestmentResolutionCalculation(
                new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: true,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 0,
                    InvestedAmountLost: investment.InvestedAmount,
                    $"Your partner in {definition.Name} disappeared with the funds."),
                ShouldSuspend: false);
        }

        var income = rng.Next(investment.WeeklyIncomeMin, investment.WeeklyIncomeMax + 1);
        return new InvestmentResolutionCalculation(
            new InvestmentResolution(
                investment.Type,
                income,
                WasLost: false,
                ExtortionPaid: 0,
                PolicePressureIncrease: 0,
                InvestedAmountLost: 0,
                $"{definition.Name} earned {income} LE this week."),
            ShouldSuspend: false);
    }
#pragma warning restore CA5394
}
