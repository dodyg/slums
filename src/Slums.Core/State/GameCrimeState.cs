namespace Slums.Core.State;

internal sealed class GameCrimeState
{
    public int TotalCrimeEarnings { get; set; }

    public int CrimesCommitted { get; set; }

    public int LastCrimeDay { get; set; }

    public bool CrimeCommittedToday { get; set; }
}
