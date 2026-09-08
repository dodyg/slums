using Microsoft.Extensions.Logging;
using Slums.Application.Content;
using Slums.Core.Content;
using Slums.Core.Diagnostics;
using Slums.Core.Endings;
using Slums.Core.World;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Infrastructure.Content;
using Slums.Narrative.Ink;

namespace Slums.Game.Content;

/// <summary>Default content bootstrap: load JSON content, cross-validate it against the Ink story, and publish the catalog.</summary>
internal sealed class ContentBootstrapper : IContentBootstrapper
{
    private readonly ILogger<ContentBootstrapper> _logger;
    private readonly IContentRepository _contentRepository;
    private readonly IGameContentCatalogProvider _contentCatalogProvider;

    public ContentBootstrapper(
        ILogger<ContentBootstrapper> logger,
        IContentRepository contentRepository,
        IGameContentCatalogProvider contentCatalogProvider)
    {
        _logger = logger;
        _contentRepository = contentRepository;
        _contentCatalogProvider = contentCatalogProvider;
    }

    public GameContentCatalog Bootstrap()
    {
        var backgrounds = _contentRepository.LoadBackgrounds();
        var jobs = _contentRepository.LoadJobs();
        var locations = _contentRepository.LoadLocations();
        var randomEvents = _contentRepository.LoadRandomEvents();
        var districtConditions = _contentRepository.LoadDistrictConditions();
        var pets = _contentRepository.LoadPets();
        var plants = _contentRepository.LoadPlants();
        var robots = _contentRepository.LoadRobots();
        var newsFlashes = _contentRepository.LoadNewsFlashes();
        var items = _contentRepository.LoadItems();
        var npcSchedules = _contentRepository.LoadNpcSchedules();
        var catalog = new GameContentCatalog(
            backgrounds,
            locations,
            jobs,
            randomEvents,
            districtConditions,
            npcSchedules,
            pets,
            plants,
            robots,
            newsFlashes,
            items);

        var knotNames = InkStoryCatalog.GetKnotNames();
        EndingKnotCatalog.ValidateKnownKnots(knotNames);

        ContentCatalogValidator.Validate(
            backgrounds,
            locations,
            jobs,
            randomEvents,
            districtConditions,
            pets,
            plants,
            knotNames,
            robots,
            newsFlashes,
            items,
            npcSchedules);

        _contentCatalogProvider.Publish(catalog);
        LogContentConfigured(_logger);
        return catalog;
    }

    private static readonly Action<ILogger, Exception?> LogContentConfiguredDelegate =
        LoggerMessage.Define(LogLevel.Information, new EventId(LogEvents.HostContentConfigured, "ContentConfigured"), "Configured content from content/data.");

    private static void LogContentConfigured(ILogger logger) => LogContentConfiguredDelegate(logger, null);
}
