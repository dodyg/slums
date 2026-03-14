using Slums.Core.Investments;

namespace Slums.Core.State;

internal sealed class GameInvestmentState
{
    public List<Investment> ActiveInvestments { get; } = [];

    public int TotalInvestmentEarnings { get; set; }

    public int WeeksSinceLastResolution { get; set; }
}
