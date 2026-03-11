namespace Slums.Application.Randomness;

public interface IRandomSource
{
    public Random SharedRandom { get; }
}