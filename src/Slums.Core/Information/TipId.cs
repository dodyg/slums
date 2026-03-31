namespace Slums.Core.Information;

public static class TipId
{
    private static int _counter;

    public static string Generate()
    {
        return $"tip_{Interlocked.Increment(ref _counter)}";
    }

    public static void Reset()
    {
        _counter = 0;
    }
}
