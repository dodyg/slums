namespace Slums.Core.Phone;

public static class PhoneMessageId
{
    private static int _counter;

    public static string Generate()
    {
        return $"msg_{Interlocked.Increment(ref _counter)}";
    }

    public static void Reset()
    {
        _counter = 0;
    }
}
