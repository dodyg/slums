using Slums.Core.Characters;

namespace Slums.Infrastructure.Persistence;

public sealed record OwnedPetSnapshot(PetType Type, int AcquiredOnDay, int LastUpkeepPaidWeek)
{
    public static OwnedPetSnapshot Capture(OwnedPet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);
        return new OwnedPetSnapshot(pet.Type, pet.AcquiredOnDay, pet.LastUpkeepPaidWeek);
    }

    public OwnedPet Restore()
    {
        return OwnedPet.Restore(Type, AcquiredOnDay, LastUpkeepPaidWeek);
    }
}
