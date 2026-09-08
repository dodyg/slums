namespace Slums.Core.Diagnostics;

/// <summary>
/// Central registry of structured-log EventIds. Every logging call site in the solution takes
/// its EventId from here so identifiers stay unique and greppable across assemblies.
/// </summary>
public static class LogEvents
{
    // Session mutation journal.
    public const int MutationGameMutation = 100;

    // Content bootstrap and validation.
    public const int ContentMissingContentFile = 1;
    public const int ContentInvalidContentJson = 2;
    public const int ContentContentReadFailure = 3;
    public const int ContentConfigured = 10;

    // Save/load persistence.
    public const int SaveReadJsonFailure = 1;
    public const int SaveReadIoFailure = 2;
    public const int SaveCompleted = 3;
    public const int SaveVersionMismatch = 4;
    public const int SaveInvalidData = 5;
    public const int SaveRestoreFailed = 6;
    public const int SavingGame = 200;
    public const int LoadingGame = 201;
    public const int GameLoaded = 202;
    public const int GameLoadFailed = 204;
    public const int LegacyRandomFallback = 205;

    // Narrative runtime.
    public const int SceneStarted = 1;
    public const int SceneCompleted = 3;
    public const int StoryEnded = 4;

    // Game host bootstrap.
    public const int HostContentConfigured = 1;
}
