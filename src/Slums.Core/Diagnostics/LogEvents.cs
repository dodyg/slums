namespace Slums.Core.Diagnostics;

/// <summary>
/// Central registry of structured-log EventIds. Each logging call site in the solution takes
/// its EventId from here so identifiers stay unique and greppable across assemblies.
/// </summary>
public static class LogEvents
{
    // Session mutation journal.
    public const int MutationGameMutation = 100;

    // Content bootstrap and validation.
    public const int ContentMissingContentFile = 200;
    public const int ContentInvalidContentJson = 201;
    public const int ContentContentReadFailure = 202;
    public const int ContentConfigured = 203;

    // Save/load persistence.
    public const int SaveReadJsonFailure = 300;
    public const int SaveReadIoFailure = 301;
    public const int SaveCompleted = 302;
    public const int SaveVersionMismatch = 303;
    public const int SaveInvalidData = 304;
    public const int SaveRestoreFailed = 305;
    public const int SavingGame = 306;
    public const int LoadingGame = 307;
    public const int GameLoaded = 308;
    public const int GameLoadFailed = 309;
    public const int LegacyRandomFallback = 310;

    // Narrative runtime.
    public const int SceneStarted = 400;
    public const int SceneCompleted = 401;
    public const int StoryEnded = 402;

    // Game host bootstrap.
    public const int HostContentConfigured = 500;

    // Run creation and randomness.
    public const int RandomSeedCreated = 600;
    public const int NewGameCreated = 601;
}
