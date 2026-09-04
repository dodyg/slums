namespace Slums.Core.Information;

internal static class ContactErosionRule
{
    internal const int MinimumTrust = 10;
    internal const int IgnoredItemThreshold = 3;

    internal static bool ShouldErode(int trust, int ignoredCount)
    {
        return trust >= MinimumTrust && ignoredCount >= IgnoredItemThreshold;
    }
}
