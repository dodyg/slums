using Microsoft.Extensions.Logging.Abstractions;
using Slums.Core.Characters;
using Slums.Core.Content;
using Slums.Core.Events;
using Slums.Core.Inventory;
using Slums.Core.Jobs;
using Slums.Core.Robotics;
using Slums.Core.World;
using Slums.Core.World.News;
using Slums.Infrastructure.Content;

namespace Slums.TestSupport;

public static class ContentCatalogTestFixture
{
    public static GameContentCatalog ConfigureCoreContent()
    {
        var repository = new JsonContentRepository(
            NullLogger<JsonContentRepository>.Instance,
            FindContentDirectory());
        var catalog = new GameContentCatalog(
            repository.LoadBackgrounds(),
            repository.LoadLocations(),
            repository.LoadJobs(),
            repository.LoadRandomEvents(),
            repository.LoadDistrictConditions(),
            repository.LoadNpcSchedules(),
            repository.LoadPets(),
            repository.LoadPlants(),
            repository.LoadRobots(),
            repository.LoadNewsFlashes(),
            repository.LoadItems());

        BackgroundRegistry.Configure(catalog.Backgrounds);
        JobRegistry.Configure(catalog.Jobs);
        WorldState.ConfigureLocations(catalog.Locations);
        RandomEventRegistry.Configure(catalog.RandomEvents);
        return catalog;
    }

    private static string FindContentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var contentDirectory = Path.Combine(directory.FullName, "content", "data");
            if (File.Exists(Path.Combine(contentDirectory, "backgrounds.json")))
            {
                return contentDirectory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository content/data directory for tests.");
    }
}
