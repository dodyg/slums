namespace Slums.Core.Characters;

public static class GenderModifiers
{
    public static int JobPayModifier(Gender gender, string jobTypeName)
    {
        return gender switch
        {
            Gender.Female => jobTypeName switch
            {
                "MicrobusDispatch" => -4,
                "MarketPorter" => -3,
                _ => 0
            },
            Gender.Male => jobTypeName switch
            {
                "HouseCleaning" => -2,
                "WorkshopSewing" => -2,
                _ => 0
            },
            _ => 0
        };
    }

    public static int JobStressModifier(Gender gender, string jobTypeName)
    {
        return gender switch
        {
            Gender.Female => jobTypeName switch
            {
                "MicrobusDispatch" => 3,
                "MarketPorter" => 2,
                _ => 0
            },
            Gender.Male => jobTypeName switch
            {
                "HouseCleaning" => 2,
                "WorkshopSewing" => 1,
                _ => 0
            },
            _ => 0
        };
    }

    public static int CrimeDetectionModifier(Gender gender, string crimeTypeName)
    {
        return gender switch
        {
            Gender.Female => crimeTypeName switch
            {
                "BulaqProtectionRacket" => 8,
                "PettyTheft" => -3,
                _ => 0
            },
            Gender.Male => crimeTypeName switch
            {
                "Robbery" => 3,
                "NetworkErrand" => -3,
                _ => 0
            },
            _ => 0
        };
    }

    public static int NpcStartingTrustModifier(Gender gender, string npcId)
    {
        return gender switch
        {
            Gender.Female => npcId switch
            {
                "NeighborMona" => 5,
                "FixerUmmKarim" => 3,
                "NurseSalma" => 3,
                "DispatcherSafaa" => 5,
                "LaundryOwnerIman" => 3,
                "WorkshopBossAbuSamir" => -3,
                "RunnerYoussef" => -3,
                _ => 0
            },
            Gender.Male => npcId switch
            {
                "NeighborMona" => -5,
                "FixerUmmKarim" => -3,
                "DispatcherSafaa" => -3,
                "LaundryOwnerIman" => -3,
                "WorkshopBossAbuSamir" => 5,
                "RunnerYoussef" => 5,
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
