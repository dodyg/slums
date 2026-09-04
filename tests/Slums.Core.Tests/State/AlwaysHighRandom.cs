namespace Slums.Core.Tests.State;

internal sealed class AlwaysHighRandom : Random
{
    public override int Next(int maxValue) => maxValue - 1;

    public override int Next(int minValue, int maxValue) => maxValue - 1;
}
