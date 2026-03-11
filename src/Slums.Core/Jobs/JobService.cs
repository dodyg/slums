using Slums.Core.Characters;
using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.Jobs;

public sealed class JobService
{
    private readonly Random _random = new();

    public JobResult PerformJob(JobShift job, PlayerCharacter player, Location currentLocation)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(currentLocation);

        if (!CanPerformJob(job, player, currentLocation, out var reason))
        {
            return JobResult.Failed(reason);
        }

        var pay = job.CalculatePay(_random);
        var energyCost = job.EnergyCost;
        if (player.Skills.GetLevel(SkillId.Physical) >= 3 &&
            (job.Type == JobType.BakeryWork || job.Type == JobType.HouseCleaning || job.Type == JobType.WorkshopSewing))
        {
            energyCost = Math.Max(0, energyCost - 5);
        }

        player.Stats.ModifyMoney(pay);
        player.Stats.ModifyEnergy(-energyCost);
        player.Stats.ModifyStress(job.StressCost);

        return JobResult.SuccessWork(pay, energyCost, job.StressCost,
            $"Worked {job.Name}. Earned {pay} LE. (-{energyCost} Energy, +{job.StressCost} Stress)");
    }

#pragma warning disable CA1822 // Methods don't access instance data but this is a service pattern
    public bool CanPerformJob(JobShift job, PlayerCharacter player, Location location, out string reason)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(location);

        if (!location.HasJobOpportunities)
        {
            reason = "No work available at this location.";
            return false;
        }

        if (player.Stats.Energy < job.MinEnergyRequired)
        {
            reason = $"Too tired. Need at least {job.MinEnergyRequired} Energy (you have {player.Stats.Energy}).";
            return false;
        }

        if (!IsJobAvailableAtLocation(job.Type, location.Id))
        {
            reason = $"{job.Name} is not available at {location.Name}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

#pragma warning disable CA1822
    public IEnumerable<JobShift> GetAvailableJobs(Location location)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(location);

        if (!location.HasJobOpportunities)
        {
            return [];
        }

        return location.Id switch
        {
            _ when location.Id == LocationId.Bakery => [JobRegistry.BakeryWork],
            _ when location.Id == LocationId.Market => [JobRegistry.HouseCleaning],
            _ when location.Id == LocationId.CallCenter => [JobRegistry.CallCenterWork],
            _ when location.Id == LocationId.Clinic => [JobRegistry.ClinicReception],
            _ when location.Id == LocationId.Workshop => [JobRegistry.WorkshopSewing],
            _ when location.Id == LocationId.Cafe => [JobRegistry.CafeService],
            _ => []
        };
    }

    private static bool IsJobAvailableAtLocation(JobType jobType, LocationId locationId)
    {
        return jobType switch
        {
            JobType.BakeryWork => locationId == LocationId.Bakery,
            JobType.HouseCleaning => locationId == LocationId.Market,
            JobType.CallCenterWork => locationId == LocationId.CallCenter,
            JobType.ClinicReception => locationId == LocationId.Clinic,
            JobType.WorkshopSewing => locationId == LocationId.Workshop,
            JobType.CafeService => locationId == LocationId.Cafe,
            _ => false
        };
    }
}
