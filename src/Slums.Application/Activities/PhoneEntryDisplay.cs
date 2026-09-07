namespace Slums.Application.Activities;

public sealed record PhoneEntryDisplay(
    string Id,
    string Label,
    string Content,
    string TypeIcon,
    bool IsEmergency,
    bool RequiresResponse,
    bool IsTip,
    int? DaysUntilExpiry,
    string SourceName);
