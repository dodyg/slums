namespace Slums.Core.State;

internal sealed class GameWorkState
{
    public int TotalHonestWorkEarnings { get; set; }

    public int HonestShiftsCompleted { get; set; }

    public int LastHonestWorkDay { get; set; }

    public int LastPublicFacingWorkDay { get; set; }
}
