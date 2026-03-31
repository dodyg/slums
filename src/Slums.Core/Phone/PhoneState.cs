namespace Slums.Core.Phone;

public sealed class PhoneState
{
    private const int DefaultCreditDays = 7;
    private const int DefaultCreditCost = 5;

    public bool HasPhone { get; private set; } = true;
    public int CreditRemaining { get; private set; } = DefaultCreditDays;
    public int CreditWeekCost { get; } = DefaultCreditCost;
    public int DaysSinceCreditRefill { get; private set; }
    public bool PhoneLost { get; private set; }
    public int? PhoneLostDay { get; private set; }
    public bool PhoneRecovered { get; private set; }

    public bool IsOperational()
    {
        return HasPhone && CreditRemaining > 0 && !PhoneLost;
    }

    public bool RefillCredit()
    {
        if (!HasPhone || PhoneLost)
        {
            return false;
        }

        CreditRemaining = DefaultCreditDays;
        DaysSinceCreditRefill = 0;
        return true;
    }

    public void DailyCreditDrain()
    {
        if (!HasPhone || PhoneLost)
        {
            return;
        }

        DaysSinceCreditRefill++;
        if (DaysSinceCreditRefill >= DefaultCreditDays && CreditRemaining > 0)
        {
            CreditRemaining = 0;
        }
        else if (DaysSinceCreditRefill < DefaultCreditDays)
        {
            CreditRemaining = DefaultCreditDays - DaysSinceCreditRefill;
        }
    }

    public void LosePhone(int day)
    {
        PhoneLost = true;
        PhoneLostDay = day;
        PhoneRecovered = false;
    }

    public void RecoverPhone()
    {
        PhoneLost = false;
        PhoneLostDay = null;
        PhoneRecovered = true;
        CreditRemaining = DefaultCreditDays;
        DaysSinceCreditRefill = 0;
    }

    public void ReplacePhone()
    {
        PhoneLost = false;
        PhoneLostDay = null;
        PhoneRecovered = false;
        HasPhone = true;
        CreditRemaining = DefaultCreditDays;
        DaysSinceCreditRefill = 0;
    }

    public void Restore(bool hasPhone, int creditRemaining, int daysSinceCreditRefill,
        bool phoneLost, int? phoneLostDay, bool phoneRecovered)
    {
        HasPhone = hasPhone;
        CreditRemaining = creditRemaining;
        DaysSinceCreditRefill = daysSinceCreditRefill;
        PhoneLost = phoneLost;
        PhoneLostDay = phoneLostDay;
        PhoneRecovered = phoneRecovered;
    }
}
