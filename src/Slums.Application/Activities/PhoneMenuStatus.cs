namespace Slums.Application.Activities;

public sealed record PhoneMenuStatus(
    IReadOnlyList<PhoneEntryDisplay> Entries,
    int CreditRemaining,
    int CreditWeekCost,
    bool PhoneLost);
