namespace Slums.Core.Investments;

public sealed class InvestmentResolutionSummary
{
    private readonly List<InvestmentResolution> _results = [];

    public IReadOnlyList<InvestmentResolution> Results => _results;

    public int TotalIncome { get; private set; }

    public int TotalLosses { get; private set; }

    public int TotalExtortion { get; private set; }

    public int LostCount { get; private set; }

    public void AddResult(InvestmentResolution result)
    {
        ArgumentNullException.ThrowIfNull(result);

        _results.Add(result);
        TotalIncome += result.Income;
        TotalExtortion += result.ExtortionPaid;

        if (result.WasLost)
        {
            TotalLosses += result.InvestedAmountLost;
            LostCount++;
        }
    }
}
