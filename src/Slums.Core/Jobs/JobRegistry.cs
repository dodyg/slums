namespace Slums.Core.Jobs;

public static class JobRegistry
{
    private static readonly JobShift DefaultBakeryWork = new()
    {
        Type = JobType.BakeryWork,
        Name = "Bakery Work (Forn)",
        Description = "Work at Al-Forn Al-Baladi beside stone ovens, hand-loaded flour bins, and a solar inverter that fails when the heat rises",
        BasePay = 19,
        EnergyCost = 25,
        StressCost = 5,
        DurationMinutes = 360,
        MinEnergyRequired = 30,
        PayVariance = 5
    };

    private static readonly JobShift DefaultHouseCleaning = new()
    {
        Type = JobType.HouseCleaning,
        Name = "House Cleaning",
        Description = "Clean homes where imported service machines are too expensive, broken, or unable to reach the corners",
        BasePay = 16,
        EnergyCost = 32,
        StressCost = 10,
        DurationMinutes = 300,
        MinEnergyRequired = 40,
        PayVariance = 3
    };

    private static readonly JobShift DefaultCallCenterWork = new()
    {
        Type = JobType.CallCenterWork,
        Name = "Call Center Shift",
        Description = "Handle customer calls while TechConnect's speech software scores every response",
        BasePay = 25,
        EnergyCost = 15,
        StressCost = 20,
        DurationMinutes = 480,
        MinEnergyRequired = 25,
        PayVariance = 7
    };

    private static readonly JobShift DefaultClinicReception = new()
    {
        Type = JobType.ClinicReception,
        Name = "Clinic Reception Shift",
        Description = "Check in patients, read cracked diagnostic displays, and keep the queue moving at Rahma Clinic",
        BasePay = 22,
        EnergyCost = 18,
        StressCost = 14,
        DurationMinutes = 420,
        MinEnergyRequired = 25,
        PayVariance = 4
    };

    private static readonly JobShift DefaultWorkshopSewing = new()
    {
        Type = JobType.WorkshopSewing,
        Name = "Garment Workshop Shift",
        Description = "Hem, press, and pack cheap garments in Abu Samir's workshop",
        BasePay = 20,
        EnergyCost = 30,
        StressCost = 8,
        DurationMinutes = 480,
        MinEnergyRequired = 35,
        PayVariance = 5
    };

    private static readonly JobShift DefaultCafeService = new()
    {
        Type = JobType.CafeService,
        Name = "Cafe Service",
        Description = "Carry tea trays beside a marked electric-taxi lane while the cafe's ordering tablet quietly rates every table",
        BasePay = 20,
        EnergyCost = 20,
        StressCost = 10,
        DurationMinutes = 360,
        MinEnergyRequired = 25,
        PayVariance = 4
    };

    private static readonly JobShift DefaultPharmacyStock = new()
    {
        Type = JobType.PharmacyStock,
        Name = "Pharmacy Stock Shift",
        Description = "Sort medicine deliveries, restock shelves, and explain why private-clinic care is not covered at Saidaleya Al-Nahda",
        BasePay = 21,
        EnergyCost = 16,
        StressCost = 12,
        DurationMinutes = 420,
        MinEnergyRequired = 25,
        PayVariance = 4
    };

    private static readonly JobShift DefaultMicrobusDispatch = new()
    {
        Type = JobType.MicrobusDispatch,
        Name = "Electric Taxi Dispatch",
        Description = "Correct route-app errors, load passengers, and keep tempers under control at the Bulaq depot",
        BasePay = 23,
        EnergyCost = 24,
        StressCost = 16,
        DurationMinutes = 480,
        MinEnergyRequired = 30,
        PayVariance = 5
    };

    private static readonly JobShift DefaultLaundryPressing = new()
    {
        Type = JobType.LaundryPressing,
        Name = "Laundry Pressing Shift",
        Description = "Press shirts, supervise half-broken folding arms, and survive the battery heat at Shubra Steam Laundry",
        BasePay = 20,
        EnergyCost = 28,
        StressCost = 9,
        DurationMinutes = 420,
        MinEnergyRequired = 30,
        PayVariance = 4
    };

    private static readonly JobShift DefaultStreetVending = new()
    {
        Type = JobType.StreetVending,
        Name = "Street Vendor Shift",
        Description = "Set up a folding table outside Midan Al-Tahrir and sell phone cases, power banks, and cheap accessories to commuters.",
        BasePay = 17,
        EnergyCost = 22,
        StressCost = 12,
        DurationMinutes = 360,
        MinEnergyRequired = 25,
        PayVariance = 6
    };

    private static readonly JobShift DefaultFishSorter = new()
    {
        Type = JobType.FishSorter,
        Name = "Fish Sorting Shift",
        Description = "Gut, scale, and sort the morning catch at Wikalet Al-Samak before the cooling cells fail and the fishwives lose patience.",
        BasePay = 18,
        EnergyCost = 28,
        StressCost = 8,
        DurationMinutes = 360,
        MinEnergyRequired = 35,
        PayVariance = 4
    };

    private static readonly JobShift DefaultMarketPorter = new()
    {
        Type = JobType.MarketPorter,
        Name = "Market Porter Shift",
        Description = "Haul crates, stack sacks, and move deliveries that couriers cannot safely bring through the narrow aisles.",
        BasePay = 17,
        EnergyCost = 30,
        StressCost = 7,
        DurationMinutes = 300,
        MinEnergyRequired = 40,
        PayVariance = 4
    };

    private static readonly JobShift DefaultRoboticsScavenging = new()
    {
        Type = JobType.RoboticsScavenging,
        Name = "Robotics Scavenging Shift",
        Description = "Strip broken delivery drones, discarded inspection cameras, and obsolete street hardware for reusable parts at Abu Samir's workshop.",
        BasePay = 24,
        EnergyCost = 24,
        StressCost = 11,
        DurationMinutes = 420,
        MinEnergyRequired = 30,
        PayVariance = 6
    };

    private static IReadOnlyList<JobShift> _jobs = [DefaultBakeryWork, DefaultHouseCleaning, DefaultCallCenterWork, DefaultClinicReception, DefaultWorkshopSewing, DefaultCafeService, DefaultPharmacyStock, DefaultMicrobusDispatch, DefaultLaundryPressing, DefaultStreetVending, DefaultFishSorter, DefaultMarketPorter, DefaultRoboticsScavenging];

    public static JobShift BakeryWork => GetJobByType(JobType.BakeryWork) ?? DefaultBakeryWork;

    public static JobShift HouseCleaning => GetJobByType(JobType.HouseCleaning) ?? DefaultHouseCleaning;

    public static JobShift CallCenterWork => GetJobByType(JobType.CallCenterWork) ?? DefaultCallCenterWork;

    public static JobShift ClinicReception => GetJobByType(JobType.ClinicReception) ?? DefaultClinicReception;

    public static JobShift WorkshopSewing => GetJobByType(JobType.WorkshopSewing) ?? DefaultWorkshopSewing;

    public static JobShift CafeService => GetJobByType(JobType.CafeService) ?? DefaultCafeService;

    public static JobShift PharmacyStock => GetJobByType(JobType.PharmacyStock) ?? DefaultPharmacyStock;

    public static JobShift MicrobusDispatch => GetJobByType(JobType.MicrobusDispatch) ?? DefaultMicrobusDispatch;

    public static JobShift LaundryPressing => GetJobByType(JobType.LaundryPressing) ?? DefaultLaundryPressing;

    public static JobShift StreetVending => GetJobByType(JobType.StreetVending) ?? DefaultStreetVending;

    public static JobShift FishSorter => GetJobByType(JobType.FishSorter) ?? DefaultFishSorter;

    public static JobShift MarketPorter => GetJobByType(JobType.MarketPorter) ?? DefaultMarketPorter;

    public static JobShift RoboticsScavenging => GetJobByType(JobType.RoboticsScavenging) ?? DefaultRoboticsScavenging;

    public static IReadOnlyList<JobShift> AllJobs => _jobs;

    public static void Configure(IEnumerable<JobShift> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        var configuredJobs = jobs.Where(static job => job is not null).ToArray();
        if (configuredJobs.Length == 0)
        {
            throw new InvalidOperationException("At least one job must be configured.");
        }

        _jobs = configuredJobs;
    }

    public static JobShift? GetJobByType(JobType type)
    {
        return _jobs.FirstOrDefault(job => job.Type == type);
    }
}
