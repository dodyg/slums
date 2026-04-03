using Slums.Core.Characters;

namespace Slums.Infrastructure.Persistence;

public sealed record OwnedPetSnapshot(
    PetType Type,
    int AcquiredOnDay,
    int LastUpkeepPaidWeek,
    bool HasBetterFilter,
    bool HasHeater,
    int DecorationsPaidWeek,
    int WaterConditionerPaidWeek)
{
    public static OwnedPetSnapshot Capture(OwnedPet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);
        return new OwnedPetSnapshot(
            pet.Type,
            pet.AcquiredOnDay,
            pet.LastUpkeepPaidWeek,
            pet.HasBetterFilter,
            pet.HasHeater,
            pet.DecorationsPaidWeek,
            pet.WaterConditionerPaidWeek);
    }

    public OwnedPet Restore()
    {
        return OwnedPet.Restore(Type, AcquiredOnDay, LastUpkeepPaidWeek, HasBetterFilter, HasHeater, DecorationsPaidWeek, WaterConditionerPaidWeek);
    }
}
