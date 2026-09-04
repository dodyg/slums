using System.Reflection;

namespace Slums.Narrative.Ink;

/// <summary>
/// Loads the compiled Ink story JSON from the filesystem artifact, falling back to the
/// embedded resource copy. Missing or invalid content is a hard failure.
/// </summary>
internal static class InkStoryLoader
{
    public static string LoadStoryJson()
    {
        var filesystemPath = Path.Combine(AppContext.BaseDirectory, "content", "ink", "main.json");
        if (File.Exists(filesystemPath))
        {
            return File.ReadAllText(filesystemPath);
        }

        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Slums.Narrative.Ink.Content.main.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not find Ink story at {filesystemPath} or as embedded resource: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
