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
        NpcId.VendorTarek => GetTarekPool(context),
        _ => throw new ArgumentOutOfRangeException(nameof(npcId))
    };

    public static string DetermineContext(NpcId npcId, NpcRelationship relationship, int policePressure, int currentDay, int honestShiftsCompleted, int crimesCommitted, int currentMoney, int motherHealth)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        var maintainingDoubleLife = honestShiftsCompleted >= 3 && crimesCommitted > 0;

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when currentMoney < 40 && relationship.Trust >= 15 => ConversationContexts.BrokeSoft,
            NpcId.LandlordHajjMahmoud when currentMoney < 40 => ConversationContexts.Broke,
            NpcId.LandlordHajjMahmoud when relationship.Trust <= -15 => ConversationContexts.Hostile,
            NpcId.LandlordHajjMahmoud when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.LandlordHajjMahmoud => ConversationContexts.Default,
            NpcId.FixerUmmKarim when maintainingDoubleLife && relationship.Trust >= 10 => ConversationContexts.DoubleLife,
            NpcId.FixerUmmKarim when relationship.Trust >= 25 => ConversationContexts.Trusted,
            NpcId.FixerUmmKarim when relationship.LastRefusalDay > 0 && currentDay - relationship.LastRefusalDay <= 3 => ConversationContexts.RecentRefusal,
            NpcId.FixerUmmKarim when relationship.RecentContactCount >= 2 => ConversationContexts.Repeat,
            NpcId.FixerUmmKarim => ConversationContexts.First,
            NpcId.OfficerKhalid when policePressure >= 70 => ConversationContexts.Hot,
            NpcId.OfficerKhalid when relationship.Trust <= -10 => ConversationContexts.Marked,
            NpcId.OfficerKhalid => ConversationContexts.Default,
            NpcId.NeighborMona when policePressure >= 70 && crimesCommitted > 0 => ConversationContexts.Heat,
            NpcId.NeighborMona when currentMoney < 40 => ConversationContexts.Lean,
            NpcId.NeighborMona when relationship.WasHelped => ConversationContexts.Helped,
            NpcId.NeighborMona when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.NeighborMona => ConversationContexts.Default,
            NpcId.NurseSalma when relationship.HasUnpaidDebt && relationship.Trust >= 15 => ConversationContexts.DebtWarm,
            NpcId.NurseSalma when relationship.HasUnpaidDebt => ConversationContexts.Debt,
            NpcId.NurseSalma when motherHealth < 40 => ConversationContexts.Urgent,
            NpcId.NurseSalma when maintainingDoubleLife => ConversationContexts.Suspicious,
            NpcId.NurseSalma when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.NurseSalma => ConversationContexts.Default,
            NpcId.WorkshopBossAbuSamir when relationship.WasEmbarrassed => ConversationContexts.Embarrassed,
            NpcId.WorkshopBossAbuSamir when relationship.Trust <= -10 => ConversationContexts.Cold,
            NpcId.WorkshopBossAbuSamir when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.WorkshopBossAbuSamir => ConversationContexts.Default,
            NpcId.CafeOwnerNadia when maintainingDoubleLife => ConversationContexts.DoubleLife,
            NpcId.CafeOwnerNadia when relationship.Trust <= -10 => ConversationContexts.Cold,
            NpcId.CafeOwnerNadia when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.CafeOwnerNadia => ConversationContexts.Default,
            NpcId.FenceHanan when relationship.Trust <= -10 => ConversationContexts.Cold,
            NpcId.FenceHanan when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.FenceHanan => ConversationContexts.Default,
            NpcId.RunnerYoussef when policePressure >= 70 => ConversationContexts.Hot,
            NpcId.RunnerYoussef when relationship.Trust >= 15 && crimesCommitted >= 2 => ConversationContexts.Embedded,
            NpcId.RunnerYoussef => ConversationContexts.Default,
            NpcId.PharmacistMariam when motherHealth < 40 => ConversationContexts.Urgent,
            NpcId.PharmacistMariam when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.PharmacistMariam => ConversationContexts.Default,
            NpcId.DispatcherSafaa when relationship.RecentContactCount >= 3 => ConversationContexts.Regular,
            NpcId.DispatcherSafaa when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.DispatcherSafaa => ConversationContexts.Default,
            NpcId.LaundryOwnerIman when currentMoney < 50 => ConversationContexts.Lean,
            NpcId.LaundryOwnerIman when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.LaundryOwnerIman => ConversationContexts.Default,
            NpcId.VendorTarek when crimesCommitted >= 3 && relationship.Trust >= 15 => "streetwise",
            NpcId.VendorTarek when relationship.Trust >= 15 => ConversationContexts.Warm,
            NpcId.VendorTarek => ConversationContexts.Default,
            _ => ConversationContexts.Default
        };
    }

    private static List<string> GetLandlordPool(string context) => context switch
    {
        ConversationContexts.BrokeSoft => GeneratePool(ConversationPoolPrefixes.LandlordBrokeSoft, 100),
        ConversationContexts.Broke => GeneratePool(ConversationPoolPrefixes.LandlordBroke, 100),
        ConversationContexts.Hostile => GeneratePool(ConversationPoolPrefixes.LandlordHostile, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.LandlordWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.LandlordDefault, 100)
    };

    private static List<string> GetFixerPool(string context) => context switch
    {
        ConversationContexts.DoubleLife => GeneratePool(ConversationPoolPrefixes.FixerDoubleLife, 100),
        ConversationContexts.Trusted => GeneratePool(ConversationPoolPrefixes.FixerTrusted, 100),
        ConversationContexts.RecentRefusal => GeneratePool(ConversationPoolPrefixes.FixerRefusal, 100),
        ConversationContexts.Repeat => GeneratePool(ConversationPoolPrefixes.FixerRepeat, 100),
        _ => GeneratePool(ConversationPoolPrefixes.FixerFirst, 100)
    };

    private static List<string> GetOfficerPool(string context) => context switch
    {
        ConversationContexts.Hot => GeneratePool(ConversationPoolPrefixes.OfficerHot, 100),
        ConversationContexts.Marked => GeneratePool(ConversationPoolPrefixes.OfficerMarked, 100),
        _ => GeneratePool(ConversationPoolPrefixes.OfficerDefault, 100)
    };

    private static List<string> GetMonaPool(string context) => context switch
    {
        ConversationContexts.Heat => GeneratePool(ConversationPoolPrefixes.MonaHeat, 100),
        ConversationContexts.Lean => GeneratePool(ConversationPoolPrefixes.MonaLean, 100),
        ConversationContexts.Helped => GeneratePool(ConversationPoolPrefixes.MonaHelped, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.MonaWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.MonaDefault, 100)
    };

    private static List<string> GetNursePool(string context) => context switch
    {
        ConversationContexts.DebtWarm => GeneratePool(ConversationPoolPrefixes.SalmaDebtWarm, 100),
        ConversationContexts.Debt => GeneratePool(ConversationPoolPrefixes.SalmaDebt, 100),
        ConversationContexts.Urgent => GeneratePool(ConversationPoolPrefixes.SalmaUrgent, 100),
        ConversationContexts.Suspicious => GeneratePool(ConversationPoolPrefixes.SalmaSuspicious, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.SalmaWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.SalmaDefault, 100)
    };

    private static List<string> GetAbuSamirPool(string context) => context switch
    {
        ConversationContexts.Embarrassed => GeneratePool(ConversationPoolPrefixes.AbuSamirEmbarrassed, 100),
        ConversationContexts.Cold => GeneratePool(ConversationPoolPrefixes.AbuSamirCold, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.AbuSamirWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.AbuSamirDefault, 100)
    };

    private static List<string> GetNadiaPool(string context) => context switch
    {
        ConversationContexts.DoubleLife => GeneratePool(ConversationPoolPrefixes.NadiaDoubleLife, 100),
        ConversationContexts.Cold => GeneratePool(ConversationPoolPrefixes.NadiaCold, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.NadiaWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.NadiaDefault, 100)
    };

    private static List<string> GetHananPool(string context) => context switch
    {
        ConversationContexts.Cold => GeneratePool(ConversationPoolPrefixes.HananCold, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.HananWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.HananDefault, 100)
    };

    private static List<string> GetYoussefPool(string context) => context switch
    {
        ConversationContexts.Hot => GeneratePool(ConversationPoolPrefixes.YoussefHot, 100),
        ConversationContexts.Embedded => GeneratePool(ConversationPoolPrefixes.YoussefEmbedded, 100),
        _ => GeneratePool(ConversationPoolPrefixes.YoussefDefault, 100)
    };

    private static List<string> GetMariamPool(string context) => context switch
    {
        ConversationContexts.Urgent => GeneratePool(ConversationPoolPrefixes.MariamUrgent, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.MariamWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.MariamDefault, 100)
    };

    private static List<string> GetSafaaPool(string context) => context switch
    {
        ConversationContexts.Regular => GeneratePool(ConversationPoolPrefixes.SafaaRegular, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.SafaaWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.SafaaDefault, 100)
    };

    private static List<string> GetImanPool(string context) => context switch
    {
        ConversationContexts.Lean => GeneratePool(ConversationPoolPrefixes.ImanLean, 100),
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.ImanWarm, 100),
        _ => GeneratePool(ConversationPoolPrefixes.ImanDefault, 100)
    };

    private static List<string> GetTarekPool(string context) => context switch
    {
        ConversationContexts.Warm => GeneratePool(ConversationPoolPrefixes.TarekWarm, 100),
        "streetwise" => GeneratePool(ConversationPoolPrefixes.TarekStreetwise, 100),
        _ => GeneratePool(ConversationPoolPrefixes.TarekDefault, 100)
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
