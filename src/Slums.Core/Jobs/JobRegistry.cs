namespace Slums.Core.Jobs;

/// <summary>Provides configured job definitions to legacy callers.</summary>
public static class JobRegistry
{
    private static IReadOnlyList<JobShift>? _jobs;

    public static JobShift BakeryWork => GetRequiredJob(JobType.BakeryWork);
    public static JobShift HouseCleaning => GetRequiredJob(JobType.HouseCleaning);
    public static JobShift CallCenterWork => GetRequiredJob(JobType.CallCenterWork);
    public static JobShift ClinicReception => GetRequiredJob(JobType.ClinicReception);
    public static JobShift WorkshopSewing => GetRequiredJob(JobType.WorkshopSewing);
    public static JobShift CafeService => GetRequiredJob(JobType.CafeService);
    public static JobShift PharmacyStock => GetRequiredJob(JobType.PharmacyStock);
    public static JobShift MicrobusDispatch => GetRequiredJob(JobType.MicrobusDispatch);
    public static JobShift LaundryPressing => GetRequiredJob(JobType.LaundryPressing);
    public static JobShift StreetVending => GetRequiredJob(JobType.StreetVending);
    public static JobShift FishSorter => GetRequiredJob(JobType.FishSorter);
    public static JobShift MarketPorter => GetRequiredJob(JobType.MarketPorter);
    public static JobShift RoboticsScavenging => GetRequiredJob(JobType.RoboticsScavenging);

    public static IReadOnlyList<JobShift> AllJobs => GetConfiguredJobs();

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
        return GetConfiguredJobs().FirstOrDefault(job => job.Type == type);
    }

    private static JobShift GetRequiredJob(JobType type)
    {
        return GetJobByType(type)
            ?? throw new InvalidOperationException($"No job configured for {type}.");
    }

    private static IReadOnlyList<JobShift> GetConfiguredJobs()
    {
        return _jobs
            ?? throw new InvalidOperationException("Job content is not configured. Configure GameContentCatalog before querying jobs.");
    }
}
