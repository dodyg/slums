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

    private static IReadOnlyList<JobShift> _jobs = [DefaultBakeryWork, DefaultHouseCleaning, DefaultCallCenterWork];

    public static JobShift BakeryWork => GetJobByType(JobType.BakeryWork) ?? DefaultBakeryWork;

    public static JobShift HouseCleaning => GetJobByType(JobType.HouseCleaning) ?? DefaultHouseCleaning;

    public static JobShift CallCenterWork => GetJobByType(JobType.CallCenterWork) ?? DefaultCallCenterWork;

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
