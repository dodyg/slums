namespace Slums.Application.Persistence;

/// <summary>Outcome of attempting to load a save.</summary>
public enum LoadGameResultKind
{
    Missing,
    Corrupt,
    Incompatible,
    Loaded
}
