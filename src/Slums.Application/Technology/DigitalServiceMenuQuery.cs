using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed record DigitalServiceMenuStatus(
    DigitalServicePreview Preview,
    bool CanPerform,
    string? UnavailabilityReason);

public sealed class DigitalServiceMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<DigitalServiceMenuStatus> GetStatuses(DigitalServiceMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new DigitalServiceMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
