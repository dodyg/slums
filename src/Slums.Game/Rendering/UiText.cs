namespace Slums.Game.Rendering;

internal static class UiText
{
    internal static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }

    internal static string FormatDuration(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes}m";
        }

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return remainingMinutes > 0 ? $"{hours}h {remainingMinutes}m" : $"{hours}h";
    }
}
