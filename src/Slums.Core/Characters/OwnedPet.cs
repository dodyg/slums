namespace Slums.Core.Characters;

public sealed class OwnedPet
{
    public PetType Type { get; init; }

    public int AcquiredOnDay { get; init; }

    public int LastUpkeepPaidWeek { get; private set; }

    public bool IsUpkeepPaidForWeek(int currentWeek)
    {
        return LastUpkeepPaidWeek >= currentWeek;
    }

    public void PayUpkeep(int currentWeek)
    {
        LastUpkeepPaidWeek = Math.Max(LastUpkeepPaidWeek, currentWeek);
    }

    public static OwnedPet Create(PetType type, int currentDay, int currentWeek)
    {
        return new OwnedPet
        {
            Type = type,
            AcquiredOnDay = currentDay,
            LastUpkeepPaidWeek = currentWeek
        };
    }

    public static OwnedPet Restore(PetType type, int acquiredOnDay, int lastUpkeepPaidWeek)
    {
        return new OwnedPet
        {
            Type = type,
            AcquiredOnDay = acquiredOnDay,
            LastUpkeepPaidWeek = lastUpkeepPaidWeek
        };
    }
}
