using Ink.Runtime;

namespace Slums.Narrative.Ink;

/// <summary>
/// Creates <see cref="Story"/> instances from compiled Ink JSON.
/// </summary>
/// <remarks>
/// The Qyl27.Ink.Engine runtime lazily initializes its static native-function registry on the
/// first story construction, assigning the registry field before it is fully populated.
/// Constructing stories concurrently from multiple threads can therefore observe a partially
/// populated registry and fail with "Failed to convert token to runtime object: ==".
/// All story construction is serialized here so the first load fully initializes the runtime
/// before any other thread can observe it. Story construction is not a hot path.
/// </remarks>
public static class InkStoryFactory
{
    private static readonly object ConstructionLock = new();

    /// <summary>
    /// Builds a runtime story from compiled Ink JSON, serialized against concurrent construction.
    /// </summary>
    /// <param name="json">The compiled Ink story JSON (inkVersion 21, e.g. inkjs 2.4.0 output).</param>
    /// <returns>A fully parsed story ready for navigation.</returns>
    public static Story Create(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        lock (ConstructionLock)
        {
            return new Story(json);
        }
    }
}
