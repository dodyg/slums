namespace Slums.Core.Jobs;

public static class JobRegistry
{
    private static readonly JobShift DefaultBakeryWork = new()
    {
        Type = JobType.BakeryWork,
        Name = "Bakery Work (Forn)",
        Description = "Work at Al-Forn Al-Baladi baking bread",
        BasePay = 18,
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
        Description = "Clean homes for families in the neighbourhood",
        BasePay = 15,
        EnergyCost = 35,
        StressCost = 10,
        DurationMinutes = 300,
        MinEnergyRequired = 40,
        PayVariance = 3
    };

    private static readonly JobShift DefaultCallCenterWork = new()
    {
        Type = JobType.CallCenterWork,
        Name = "Call Center Shift",
        Description = "Handle customer calls at TechConnect",
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
        Description = "Check in patients and keep the queue moving at Rahma Clinic",
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
        Description = "Carry tea trays and clear tables at Ahwa El-Galaa",
        BasePay = 19,
        EnergyCost = 20,
        StressCost = 11,
        DurationMinutes = 360,
        MinEnergyRequired = 25,
        PayVariance = 4
    };

    private static readonly JobShift DefaultPharmacyStock = new()
    {
        Type = JobType.PharmacyStock,
        Name = "Pharmacy Stock Shift",
        Description = "Sort deliveries, restock shelves, and keep prices straight at Saidaleya Al-Nahda",
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
        Name = "Microbus Dispatch",
        Description = "Call routes, load passengers, and keep tempers under control at the Bulaq depot",
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
        Description = "Press shirts, fold sheets, and survive the heat at Shubra Steam Laundry",
        BasePay = 20,
        EnergyCost = 28,
        StressCost = 9,
        DurationMinutes = 420,
        MinEnergyRequired = 30,
        PayVariance = 4
    };

    private static IReadOnlyList<JobShift> _jobs = [DefaultBakeryWork, DefaultHouseCleaning, DefaultCallCenterWork, DefaultClinicReception, DefaultWorkshopSewing, DefaultCafeService, DefaultPharmacyStock, DefaultMicrobusDispatch, DefaultLaundryPressing];

    public static JobShift BakeryWork => GetJobByType(JobType.BakeryWork) ?? DefaultBakeryWork;

    public static JobShift HouseCleaning => GetJobByType(JobType.HouseCleaning) ?? DefaultHouseCleaning;

    public static JobShift CallCenterWork => GetJobByType(JobType.CallCenterWork) ?? DefaultCallCenterWork;

    public static JobShift ClinicReception => GetJobByType(JobType.ClinicReception) ?? DefaultClinicReception;

    public static JobShift WorkshopSewing => GetJobByType(JobType.WorkshopSewing) ?? DefaultWorkshopSewing;

    public static JobShift CafeService => GetJobByType(JobType.CafeService) ?? DefaultCafeService;

    public static JobShift PharmacyStock => GetJobByType(JobType.PharmacyStock) ?? DefaultPharmacyStock;

    public static JobShift MicrobusDispatch => GetJobByType(JobType.MicrobusDispatch) ?? DefaultMicrobusDispatch;

    public static JobShift LaundryPressing => GetJobByType(JobType.LaundryPressing) ?? DefaultLaundryPressing;

    public static IReadOnlyList<JobShift> AllJobs => _jobs;

    public static void Configure(IEnumerable<JobShift> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        var configuredJobs = jobs.Where(static job => job is not null).ToArray();
        if (configuredJobs.Length > 0)
        {
            _jobs = configuredJobs;
        }
    }

    public static JobShift? GetJobByType(JobType type)
    {
        return _jobs.FirstOrDefault(job => job.Type == type);
    }
}
