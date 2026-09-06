using Slums.Core.World;

namespace Slums.Core.Investments;

public sealed record Investment
{
    public InvestmentType Type { get; init; }
    public int InvestedAmount { get; init; }
    public int WeeklyIncomeMin { get; init; }
    public int WeeklyIncomeMax { get; init; }
    public int WeeksActive { get; private set; }
    public InvestmentRiskProfile RiskProfile { get; init; } = InvestmentRiskProfile.Low;
    public bool IsSuspended { get; private set; }
    public DistrictId PurchaseDistrict { get; init; }

    public Investment(
        InvestmentType type,
        int investedAmount,
        int weeklyIncomeMin,
        int weeklyIncomeMax,
        InvestmentRiskProfile riskProfile,
        DistrictId purchaseDistrict = DistrictId.Imbaba)
    {
        Type = type;
        InvestedAmount = investedAmount;
        WeeklyIncomeMin = weeklyIncomeMin;
        WeeklyIncomeMax = weeklyIncomeMax;
        RiskProfile = riskProfile;
        PurchaseDistrict = purchaseDistrict;
        WeeksActive = 0;
        IsSuspended = false;
    }

    public void IncrementWeek()
    {
        WeeksActive++;
    }

    public void Suspend()
    {
        IsSuspended = true;
    }

    public void Unsuspend()
    {
        IsSuspended = false;
    }

    public InvestmentSnapshot CreateSnapshot()
    {
        return new InvestmentSnapshot(
            Type,
            InvestedAmount,
            WeeklyIncomeMin,
            WeeklyIncomeMax,
            WeeksActive,
            IsSuspended,
            PurchaseDistrict);
    }

    public static Investment Restore(InvestmentSnapshot snapshot, InvestmentRiskProfile riskProfile)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(riskProfile);

        return new Investment(
            snapshot.Type,
            snapshot.InvestedAmount,
            snapshot.WeeklyIncomeMin,
            snapshot.WeeklyIncomeMax,
            riskProfile,
            snapshot.PurchaseDistrict)
        {
            WeeksActive = Math.Max(0, snapshot.WeeksActive),
            IsSuspended = snapshot.IsSuspended
        };
    }
}
