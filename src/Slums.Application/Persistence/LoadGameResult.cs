namespace Slums.Application.Persistence;

/// <summary>Outcome of attempting to load a save.</summary>
public enum LoadGameResultKind
{
    /// <summary>No save file exists for the slot.</summary>
    Missing,

    /// <summary>The save file exists but could not be read, parsed, or validated.</summary>
    Corrupt,

    /// <summary>The save file is from an incompatible save version.</summary>
    Incompatible,

    /// <summary>The save loaded successfully.</summary>
    Loaded
}

/// <summary>
/// Typed result of a load attempt, distinguishing missing, corrupt, incompatible, and loaded
/// saves so the UI can react appropriately instead of treating every failure as "no save".
/// </summary>
public sealed record LoadGameResult(LoadGameResultKind Kind, LoadedGameSession? Session, string? Detail)
{
    public static LoadGameResult Missing() => new(LoadGameResultKind.Missing, null, null);

    public static LoadGameResult Corrupt(string detail) => new(LoadGameResultKind.Corrupt, null, detail);

    public static LoadGameResult Incompatible(int foundVersion, int expectedVersion)
    {
        return new LoadGameResult(LoadGameResultKind.Incompatible, null, $"Save version {foundVersion} is not compatible with the current version {expectedVersion}.");
    }

    public static LoadGameResult Loaded(LoadedGameSession session) => new(LoadGameResultKind.Loaded, session, null);
}
