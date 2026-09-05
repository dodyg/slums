namespace Slums.Application.Technology;

public sealed record TechnicalRepairMenuStatus(
    Slums.Core.Technology.TechnicalRepairPreview Preview,
    bool CanPerform,
    string? UnavailabilityReason);

public sealed class TechnicalRepairMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TechnicalRepairMenuStatus> GetStatuses(TechnicalRepairMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new TechnicalRepairMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
