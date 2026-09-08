using Slums.TestSupport;
using TUnit.Core;

namespace Slums.TestAssembly;

internal static class TestAssemblyContentSetup
{
    /// <summary>
    /// Eagerly loads and validates the repository-owned JSON content once per test assembly
    /// so content problems surface before any test executes.
    /// </summary>
    [Before(HookType.Assembly)]
    public static void LoadContent()
    {
        _ = TestContent.Catalog;
    }
}
