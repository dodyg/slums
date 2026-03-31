namespace Slums.Core.Economy;

public enum DebtSource
{
    NeighborLoan,
    LandlordAdvance,
    LoanShark,
    CommunityMutualAid
}

public enum DebtCollectionState
{
    Current,
    Overdue,
    Escalating,
    Critical
}
