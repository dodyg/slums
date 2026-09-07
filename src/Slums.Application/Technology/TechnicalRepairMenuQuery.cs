namespace Slums.Application.Technology;

public sealed class TechnicalRepairMenuQuery
{
    public IReadOnlyList<TechnicalRepairMenuStatus> GetStatuses(TechnicalRepairMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new TechnicalRepairMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
