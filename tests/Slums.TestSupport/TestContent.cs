using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Content;
using Slums.Core.Characters;
using Slums.Core.Content;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Core.World.News;
using Slums.Core.Investments;
using Slums.Core.Technology;
using Slums.Infrastructure.Content;

namespace Slums.TestSupport;

/// <summary>
/// Lazily loads the repository-owned JSON content exactly once per test process and hands
/// the resulting immutable <see cref="GameContentCatalog"/> to every consumer. The catalog is
/// never mutated, so sharing one instance across parallel tests is safe.
/// </summary>
public static class TestContent
{
    private static readonly Lazy<GameContentCatalog> CatalogLazy = new(LoadCatalog, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Gets the shared, immutable content catalog loaded from <c>content/data</c>.</summary>
    public static GameContentCatalog Catalog => CatalogLazy.Value;

    /// <summary>
    /// Builds a catalog identical to the shared one except for the supplied overrides, for tests
    /// that need custom news flashes, inventory items, or NPC schedules alongside the standard world content.
    /// </summary>
    public static GameContentCatalog CatalogWith(
        IEnumerable<NewsFlashDefinition>? newsFlashes = null,
        IEnumerable<ItemDefinition>? items = null,
        IEnumerable<NpcScheduleDefinition>? npcSchedules = null,
        IEnumerable<InvestmentDefinition>? investments = null,
        IEnumerable<DigitalServiceActionDefinition>? digitalServices = null,
        IEnumerable<TechnicalRepairActionDefinition>? technicalRepairs = null)
    {
        return new GameContentCatalog(
            Catalog.Backgrounds,
            Catalog.Locations,
            Catalog.Jobs,
            Catalog.RandomEvents,
            Catalog.DistrictConditions,
            npcSchedules ?? Catalog.NpcSchedules,
            Catalog.Pets,
            Catalog.Plants,
            Catalog.Robots,
            newsFlashes ?? Catalog.NewsFlashes,
            items ?? Catalog.Items,
            investments ?? Catalog.Investments,
            digitalServices ?? Catalog.DigitalServices,
            technicalRepairs ?? Catalog.TechnicalRepairs);
    }

    /// <summary>Creates a content catalog provider pre-published with the shared catalog.</summary>
    public static IGameContentCatalogProvider CreateProvider()
    {
        var provider = new GameContentCatalogProvider();
        provider.Publish(Catalog);
        return provider;
    }

    private static GameContentCatalog LoadCatalog()
    {
        var repository = new JsonContentRepository(
            NullLogger<JsonContentRepository>.Instance,
            FindContentDirectory());
        return new GameContentCatalog(
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
            repository.LoadItems(),
            repository.LoadInvestments(),
            repository.LoadDigitalServices(),
            repository.LoadTechnicalRepairs());
    }

    /// <summary>Walks upward from the test output directory until the repository content folder is found.</summary>
    public static string FindContentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var contentDirectory = Path.Combine(directory.FullName, "content", "data");
            if (File.Exists(Path.Combine(contentDirectory, "backgrounds.json")))
            {
                return contentDirectory;
            }

            directory = directory.Parent!;
        }

        throw new DirectoryNotFoundException("Could not locate the repository content/data directory for tests.");
    }
}
