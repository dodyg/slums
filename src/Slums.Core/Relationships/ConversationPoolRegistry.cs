namespace Slums.Core.Relationships;

public static class ConversationPoolRegistry
{
    public static IReadOnlyList<string> GetConversationPool(NpcId npcId, string context) => npcId switch
    {
        NpcId.LandlordHajjMahmoud => GetLandlordPool(context),
        NpcId.FixerUmmKarim => GetFixerPool(context),
        NpcId.OfficerKhalid => GetOfficerPool(context),
        NpcId.NeighborMona => GetMonaPool(context),
        NpcId.NurseSalma => GetNursePool(context),
        NpcId.WorkshopBossAbuSamir => GetAbuSamirPool(context),
        NpcId.CafeOwnerNadia => GetNadiaPool(context),
        NpcId.FenceHanan => GetHananPool(context),
        NpcId.RunnerYoussef => GetYoussefPool(context),
        NpcId.PharmacistMariam => GetMariamPool(context),
        NpcId.DispatcherSafaa => GetSafaaPool(context),
        NpcId.LaundryOwnerIman => GetImanPool(context),
        _ => throw new ArgumentOutOfRangeException(nameof(npcId))
    };

    public static string DetermineContext(NpcId npcId, NpcRelationship relationship, int policePressure, int currentDay, int honestShiftsCompleted, int crimesCommitted, int currentMoney, int motherHealth)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        var maintainingDoubleLife = honestShiftsCompleted >= 3 && crimesCommitted > 0;

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when currentMoney < 40 && relationship.Trust >= 15 => "broke_soft",
            NpcId.LandlordHajjMahmoud when currentMoney < 40 => "broke",
            NpcId.LandlordHajjMahmoud when relationship.Trust <= -15 => "hostile",
            NpcId.LandlordHajjMahmoud when relationship.Trust >= 15 => "warm",
            NpcId.LandlordHajjMahmoud => "default",
            NpcId.FixerUmmKarim when maintainingDoubleLife && relationship.Trust >= 10 => "double_life",
            NpcId.FixerUmmKarim when relationship.Trust >= 25 => "trusted",
            NpcId.FixerUmmKarim when relationship.LastRefusalDay > 0 && currentDay - relationship.LastRefusalDay <= 3 => "recent_refusal",
            NpcId.FixerUmmKarim when relationship.RecentContactCount >= 2 => "repeat",
            NpcId.FixerUmmKarim => "first",
            NpcId.OfficerKhalid when policePressure >= 70 => "hot",
            NpcId.OfficerKhalid when relationship.Trust <= -10 => "marked",
            NpcId.OfficerKhalid => "default",
            NpcId.NeighborMona when policePressure >= 70 && crimesCommitted > 0 => "heat",
            NpcId.NeighborMona when currentMoney < 40 => "lean",
            NpcId.NeighborMona when relationship.WasHelped => "helped",
            NpcId.NeighborMona when relationship.Trust >= 15 => "warm",
            NpcId.NeighborMona => "default",
            NpcId.NurseSalma when relationship.HasUnpaidDebt && relationship.Trust >= 15 => "debt_warm",
            NpcId.NurseSalma when relationship.HasUnpaidDebt => "debt",
            NpcId.NurseSalma when motherHealth < 40 => "urgent",
            NpcId.NurseSalma when maintainingDoubleLife => "suspicious",
            NpcId.NurseSalma when relationship.Trust >= 15 => "warm",
            NpcId.NurseSalma => "default",
            NpcId.WorkshopBossAbuSamir when relationship.WasEmbarrassed => "embarrassed",
            NpcId.WorkshopBossAbuSamir when relationship.Trust <= -10 => "cold",
            NpcId.WorkshopBossAbuSamir when relationship.Trust >= 15 => "warm",
            NpcId.WorkshopBossAbuSamir => "default",
            NpcId.CafeOwnerNadia when maintainingDoubleLife => "double_life",
            NpcId.CafeOwnerNadia when relationship.Trust <= -10 => "cold",
            NpcId.CafeOwnerNadia when relationship.Trust >= 15 => "warm",
            NpcId.CafeOwnerNadia => "default",
            NpcId.FenceHanan when relationship.Trust <= -10 => "cold",
            NpcId.FenceHanan when relationship.Trust >= 15 => "warm",
            NpcId.FenceHanan => "default",
            NpcId.RunnerYoussef when policePressure >= 70 => "hot",
            NpcId.RunnerYoussef when relationship.Trust >= 15 && crimesCommitted >= 2 => "embedded",
            NpcId.RunnerYoussef => "default",
            NpcId.PharmacistMariam when motherHealth < 40 => "urgent",
            NpcId.PharmacistMariam when relationship.Trust >= 15 => "warm",
            NpcId.PharmacistMariam => "default",
            NpcId.DispatcherSafaa when relationship.RecentContactCount >= 3 => "regular",
            NpcId.DispatcherSafaa when relationship.Trust >= 15 => "warm",
            NpcId.DispatcherSafaa => "default",
            NpcId.LaundryOwnerIman when currentMoney < 50 => "lean",
            NpcId.LaundryOwnerIman when relationship.Trust >= 15 => "warm",
            NpcId.LaundryOwnerIman => "default",
            _ => "default"
        };
    }

    private static List<string> GetLandlordPool(string context) => context switch
    {
        "broke_soft" => GeneratePool("landlord_broke_soft", 100),
        "broke" => GeneratePool("landlord_broke", 100),
        "hostile" => GeneratePool("landlord_hostile", 100),
        "warm" => GeneratePool("landlord_warm", 100),
        _ => GeneratePool("landlord_default", 100)
    };

    private static List<string> GetFixerPool(string context) => context switch
    {
        "double_life" => GeneratePool("fixer_double_life", 100),
        "trusted" => GeneratePool("fixer_trusted", 100),
        "recent_refusal" => GeneratePool("fixer_refusal", 100),
        "repeat" => GeneratePool("fixer_repeat", 100),
        _ => GeneratePool("fixer_first", 100)
    };

    private static List<string> GetOfficerPool(string context) => context switch
    {
        "hot" => GeneratePool("officer_hot", 100),
        "marked" => GeneratePool("officer_marked", 100),
        _ => GeneratePool("officer_default", 100)
    };

    private static List<string> GetMonaPool(string context) => context switch
    {
        "heat" => GeneratePool("mona_heat", 100),
        "lean" => GeneratePool("mona_lean", 100),
        "helped" => GeneratePool("mona_helped", 100),
        "warm" => GeneratePool("mona_warm", 100),
        _ => GeneratePool("mona_default", 100)
    };

    private static List<string> GetNursePool(string context) => context switch
    {
        "debt_warm" => GeneratePool("salma_debt_warm", 100),
        "debt" => GeneratePool("salma_debt", 100),
        "urgent" => GeneratePool("salma_urgent", 100),
        "suspicious" => GeneratePool("salma_suspicious", 100),
        "warm" => GeneratePool("salma_warm", 100),
        _ => GeneratePool("salma_default", 100)
    };

    private static List<string> GetAbuSamirPool(string context) => context switch
    {
        "embarrassed" => GeneratePool("abu_samir_embarrassed", 100),
        "cold" => GeneratePool("abu_samir_cold", 100),
        "warm" => GeneratePool("abu_samir_warm", 100),
        _ => GeneratePool("abu_samir_default", 100)
    };

    private static List<string> GetNadiaPool(string context) => context switch
    {
        "double_life" => GeneratePool("nadia_double_life", 100),
        "cold" => GeneratePool("nadia_cold", 100),
        "warm" => GeneratePool("nadia_warm", 100),
        _ => GeneratePool("nadia_default", 100)
    };

    private static List<string> GetHananPool(string context) => context switch
    {
        "cold" => GeneratePool("hanan_cold", 100),
        "warm" => GeneratePool("hanan_warm", 100),
        _ => GeneratePool("hanan_default", 100)
    };

    private static List<string> GetYoussefPool(string context) => context switch
    {
        "hot" => GeneratePool("youssef_hot", 100),
        "embedded" => GeneratePool("youssef_embedded", 100),
        _ => GeneratePool("youssef_default", 100)
    };

    private static List<string> GetMariamPool(string context) => context switch
    {
        "urgent" => GeneratePool("mariam_urgent", 100),
        "warm" => GeneratePool("mariam_warm", 100),
        _ => GeneratePool("mariam_default", 100)
    };

    private static List<string> GetSafaaPool(string context) => context switch
    {
        "regular" => GeneratePool("safaa_regular", 100),
        "warm" => GeneratePool("safaa_warm", 100),
        _ => GeneratePool("safaa_default", 100)
    };

    private static List<string> GetImanPool(string context) => context switch
    {
        "lean" => GeneratePool("iman_lean", 100),
        "warm" => GeneratePool("iman_warm", 100),
        _ => GeneratePool("iman_default", 100)
    };

    private static List<string> GeneratePool(string prefix, int count)
    {
        var pool = new List<string>(count);
        for (var i = 1; i <= count; i++)
        {
            pool.Add($"{prefix}_{i}");
        }
        return pool;
    }
}
