namespace Slums.Core.Tests.Investments;

internal sealed class SequenceRandom : Random
{
    private readonly Queue<double> _doubleValues;
    private readonly Queue<int> _intValues;

    public SequenceRandom(IEnumerable<double>? doubleValues = null, IEnumerable<int>? intValues = null)
    {
        _doubleValues = new Queue<double>(doubleValues ?? []);
        _intValues = new Queue<int>(intValues ?? []);
    }

    public override double NextDouble()
    {
        return _doubleValues.Count > 0 ? _doubleValues.Dequeue() : 0.99;
    }

    public override int Next(int minValue, int maxValue)
    {
        if (_intValues.Count == 0)
        {
            return minValue;
        }

        var next = _intValues.Dequeue();
        return Math.Clamp(next, minValue, maxValue - 1);
    }
}
