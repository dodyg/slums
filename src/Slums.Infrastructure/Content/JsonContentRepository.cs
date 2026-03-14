using System.Text.Json;
using Microsoft.Extensions.Logging;
using Slums.Application.Content;
using Slums.Core.Characters;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Content;

public sealed class JsonContentRepository : IContentRepository
{
    private readonly ILogger<JsonContentRepository> _logger;
    private readonly string _contentDirectory;

    public JsonContentRepository(ILogger<JsonContentRepository> logger, string? contentDirectory = null)
    {
        _logger = logger;
        _contentDirectory = contentDirectory ?? Path.Combine(AppContext.BaseDirectory, "content", "data");
    }

    public IReadOnlyList<Background> LoadBackgrounds()
    {
        return Load(Path.Combine(_contentDirectory, "backgrounds.json"), ContentJsonContext.Default.ListBackground);
    }

    public IReadOnlyList<Location> LoadLocations()
    {
        return Load(Path.Combine(_contentDirectory, "locations.json"), ContentJsonContext.Default.ListLocation);
    }

    public IReadOnlyList<JobShift> LoadJobs()
    {
        return Load(Path.Combine(_contentDirectory, "jobs.json"), ContentJsonContext.Default.ListJobShift);
    }

    public IReadOnlyList<RandomEvent> LoadRandomEvents()
    {
        var definitions = Load(Path.Combine(_contentDirectory, "random_events.json"), ContentJsonContext.Default.ListRandomEventDefinition);
        return definitions.Select(MapRandomEvent).ToArray();
    }

    public IReadOnlyList<DistrictConditionDefinition> LoadDistrictConditions()
    {
        return Load(Path.Combine(_contentDirectory, "district_conditions.json"), ContentJsonContext.Default.ListDistrictConditionDefinition);
    }

    private List<T> Load<T>(string path, System.Text.Json.Serialization.Metadata.JsonTypeInfo<List<T>> jsonTypeInfo)
    {
        if (!File.Exists(path))
        {
            LogMissingContentFile(_logger, path);
            throw new ContentLoadException($"Content file not found: {path}");
        }

        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize(stream, jsonTypeInfo)
                ?? throw new ContentLoadException($"Content file {path} did not deserialize into a valid payload.");
        }
        catch (JsonException exception)
        {
            LogInvalidContentJson(_logger, path, exception);
            throw new ContentLoadException($"Invalid JSON in content file {path}.", exception);
        }
        catch (IOException exception)
        {
            LogContentReadFailure(_logger, path, exception);
            throw new ContentLoadException($"Failed to read content file {path}.", exception);
        }
    }

    private static RandomEvent MapRandomEvent(RandomEventDefinition definition)
    {
        return new RandomEvent(
            definition.Id,
            definition.Description,
            new RandomEventEffect
            {
                MoneyChange = definition.MoneyChange,
                HealthChange = definition.HealthChange,
                EnergyChange = definition.EnergyChange,
                HungerChange = definition.HungerChange,
                StressChange = definition.StressChange,
                PolicePressureChange = definition.PolicePressureChange,
                MotherHealthChange = definition.MotherHealthChange,
                FoodChange = definition.FoodChange,
                InkKnot = definition.InkKnot
            },
            definition.MinDay,
            definition.Weight,
            CreateCondition(definition.ConditionId));
    }

    private static Func<GameSession, bool>? CreateCondition(string? conditionId)
    {
        return conditionId switch
        {
            null or "" => null,
            "mother_health_below_50" => static state => state.Player.Household.MotherHealth < 50,
            "high_police_pressure" => static state => state.PolicePressure >= 60,
            "at_home" => static state => state.World.CurrentLocationId == LocationId.Home,
            "in_imbaba" => static state => state.World.CurrentDistrict == DistrictId.Imbaba,
            "at_market" => static state => state.World.CurrentLocationId == LocationId.Market,
            "at_bakery" => static state => state.World.CurrentLocationId == LocationId.Bakery,
            "at_clinic" => static state => state.World.CurrentLocationId == LocationId.Clinic,
            "at_workshop" => static state => state.World.CurrentLocationId == LocationId.Workshop,
            "at_cafe" => static state => state.World.CurrentLocationId == LocationId.Cafe,
            "at_call_center" => static state => state.World.CurrentLocationId == LocationId.CallCenter,
            "at_pharmacy" => static state => state.World.CurrentLocationId == LocationId.Pharmacy,
            "at_depot" => static state => state.World.CurrentLocationId == LocationId.Depot,
            "at_laundry" => static state => state.World.CurrentLocationId == LocationId.Laundry,
            "in_dokki" => static state => state.World.CurrentDistrict == DistrictId.Dokki,
            "in_ard_al_liwa" => static state => state.World.CurrentDistrict == DistrictId.ArdAlLiwa,
            "in_bulaq_al_dakrour" => static state => state.World.CurrentDistrict == DistrictId.BulaqAlDakrour,
            "in_shubra" => static state => state.World.CurrentDistrict == DistrictId.Shubra,
            "in_downtown_cairo" => static state => state.World.CurrentDistrict == DistrictId.DowntownCairo,
            "dokki_checkpoint_seen" => static state => state.GetEventCount("DokkiCheckpointSweep") > 0,
            "imbaba_stressed" => static state => state.World.CurrentDistrict == DistrictId.Imbaba && state.Player.Stats.Stress >= 35,
            "ard_al_liwa_low_money" => static state => state.World.CurrentDistrict == DistrictId.ArdAlLiwa && state.Player.Stats.Money < 120,
            "shubra_low_money" => static state => state.World.CurrentDistrict == DistrictId.Shubra && state.Player.Stats.Money < 120,
            "home_low_money" => static state => state.World.CurrentLocationId == LocationId.Home && state.Player.Stats.Money < 60,
            "sudanese_refugee_home" => static state => state.Player.BackgroundType == BackgroundType.SudaneseRefugee && state.World.CurrentDistrict == DistrictId.Imbaba,
            _ => throw new ContentLoadException($"Unknown random event condition id '{conditionId}'.")
        };
    }

    private static readonly Action<ILogger, string, Exception?> LogMissingContentFileDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "MissingContentFile"), "Content file not found: {Path}");

    private static readonly Action<ILogger, string, Exception?> LogInvalidContentJsonDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, "InvalidContentJson"), "Invalid JSON in content file {Path}");

    private static readonly Action<ILogger, string, Exception?> LogContentReadFailureDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, "ContentReadFailure"), "Failed to read content file {Path}");

    private static void LogMissingContentFile(ILogger logger, string path) => LogMissingContentFileDelegate(logger, path, null);

    private static void LogInvalidContentJson(ILogger logger, string path, Exception exception) => LogInvalidContentJsonDelegate(logger, path, exception);

    private static void LogContentReadFailure(ILogger logger, string path, Exception exception) => LogContentReadFailureDelegate(logger, path, exception);
}
