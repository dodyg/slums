using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;

namespace Slums.Core.Characters;

public static class GenderModifiers
{
    public static int JobPayModifier(Gender gender, JobType jobType)
    {
        return gender switch
        {
            Gender.Female => jobType switch
            {
                JobType.MicrobusDispatch => -4,
                JobType.MarketPorter => -3,
                _ => 0
            },
            Gender.Male => jobType switch
            {
                JobType.HouseCleaning => -2,
                JobType.WorkshopSewing => -2,
                _ => 0
            },
            _ => 0
        };
    }

    public static int JobStressModifier(Gender gender, JobType jobType)
    {
        return gender switch
        {
            Gender.Female => jobType switch
            {
                JobType.MicrobusDispatch => 3,
                JobType.MarketPorter => 2,
                _ => 0
            },
            Gender.Male => jobType switch
            {
                JobType.HouseCleaning => 2,
                JobType.WorkshopSewing => 1,
                _ => 0
            },
            _ => 0
        };
    }

    public static int CrimeDetectionModifier(Gender gender, CrimeType crimeType)
    {
        return gender switch
        {
            Gender.Female => crimeType switch
            {
                CrimeType.BulaqProtectionRacket => 8,
                CrimeType.PettyTheft => -3,
                _ => 0
            },
            Gender.Male => crimeType switch
            {
                CrimeType.Robbery => 3,
                CrimeType.NetworkErrand => -3,
                _ => 0
            },
            _ => 0
        };
    }

    public static int NpcStartingTrustModifier(Gender gender, NpcId npcId)
    {
        return gender switch
        {
            Gender.Female => npcId switch
            {
                NpcId.NeighborMona => 5,
                NpcId.FixerUmmKarim => 3,
                NpcId.NurseSalma => 3,
                NpcId.DispatcherSafaa => 5,
                NpcId.LaundryOwnerIman => 3,
                NpcId.WorkshopBossAbuSamir => -3,
                NpcId.RunnerYoussef => -3,
                _ => 0
            },
            Gender.Male => npcId switch
            {
                NpcId.NeighborMona => -5,
                NpcId.FixerUmmKarim => -3,
                NpcId.DispatcherSafaa => -3,
                NpcId.LaundryOwnerIman => -3,
                NpcId.WorkshopBossAbuSamir => 5,
                NpcId.RunnerYoussef => 5,
                _ => 0
            },
            _ => 0
        };
    }

    public static int DailyStressModifier(Gender gender)
    {
        return gender == Gender.Female ? 1 : 0;
    }

    public static int PhysicalJobEnergyDrain(Gender gender)
    {
        return gender == Gender.Male ? 2 : 0;
    }

    public static string DefaultName(Gender gender)
    {
        return gender == Gender.Male ? "Karim" : "Amira";
    }
}
