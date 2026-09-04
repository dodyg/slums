namespace Slums.Core.State;

/// <summary>Coordinates the ordered daily resolution steps for a game session.</summary>
internal static class EndOfDayPipeline
{
    internal static void Run(GameSession session, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.ResolveEndOfDay(random);
    }
}
