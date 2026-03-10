namespace Slums.Core.Jobs;

public static class JobRegistry
{
    public static readonly JobShift BakeryWork = new()
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

    public static readonly JobShift HouseCleaning = new()
    {
        Type = JobType.HouseCleaning,
        Name = "House Cleaning",
        Description = "Clean homes in Dokki district",
        BasePay = 15,
        EnergyCost = 35,
        StressCost = 10,
        DurationMinutes = 300,
        MinEnergyRequired = 40,
        PayVariance = 3
    };

    public static readonly JobShift CallCenterWork = new()
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

    public static IReadOnlyList<JobShift> AllJobs => [BakeryWork, HouseCleaning, CallCenterWork];

    public static JobShift? GetJobByType(JobType type)
    {
        return type switch
        {
            JobType.BakeryWork => BakeryWork,
            JobType.HouseCleaning => HouseCleaning,
            JobType.CallCenterWork => CallCenterWork,
            _ => null
        };
    }
}
