using Slums.TestSupport;
using TUnit.Core;

namespace Slums.TestAssembly;

internal static class TestAssemblyContentSetup
{
    [Before(HookType.Assembly)]
    public static void ConfigureContent()
    {
        ContentCatalogTestFixture.ConfigureCoreContent();
    }
}
