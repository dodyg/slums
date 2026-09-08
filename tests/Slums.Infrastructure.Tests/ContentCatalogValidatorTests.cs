using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Events;
using Slums.Core.Inventory;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.World;
using Slums.Core.World.News;
using Slums.Core.Investments;
using Slums.Core.Technology;
using Slums.Infrastructure.Content;
using TUnit;

namespace Slums.Infrastructure.Tests;

internal sealed class ContentCatalogValidatorTests
{
    [Test]
    public async Task Validate_WithCompleteCatalog_DoesNotThrow()
    {
        var catalog = BuildValidCatalog();

        var act = () => Validate(catalog);

        act.Should().NotThrow();
    }

    [Test]
    public async Task Validate_WithWorldEnrichmentCatalog_DoesNotThrow()
    {
        var news = new NewsFlashDefinition
        {
            Id = "news",
            Headline = "Headline",
            Body = "Body",
            SourceLabel = "Source",
            MinimumDay = 1,
            Weight = 1,
            DurationDays = 2,
            AffectedDistricts = [DistrictId.Imbaba],
            Responses = [new NewsResponseDefinition
            {
                Id = "prepare",
                Label = "Prepare",
                Type = NewsResponseType.Prepare,
                RequiredItemId = "papers",
                RequiredItemQuantity = 1
            }]
        };
        var items = new[] { new ItemDefinition { Id = "papers", Name = "Papers", Description = "Documents", MaximumQuantity = 1 } };
        var schedules = new[] { new NpcScheduleDefinition { Npc = NpcId.NeighborMona, Days = [Slums.Core.Clock.GameDayOfWeek.Saturday], StartMinute = 360, EndMinute = 600, Location = LocationId.Home, AbsenceReason = "At home." } };

        var catalog = BuildValidCatalog() with
        {
            NewsFlashes = [news],
            Items = items,
            NpcSchedules = schedules
        };

        var act = () => Validate(catalog);

        act.Should().NotThrow();
    }

    [Test]
    public void Validate_RepositoryCatalog_WithAllOptionalCatalogs_DoesNotThrow()
    {
        var contentDirectory = ResolveRepositoryContentDirectory();
        var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);
        var backgrounds = repository.LoadBackgrounds();
        var locations = repository.LoadLocations();
        var jobs = repository.LoadJobs();
        var randomEvents = repository.LoadRandomEvents();
        var districtConditions = repository.LoadDistrictConditions();
        var pets = repository.LoadPets();
        var plants = repository.LoadPlants();
        var robots = repository.LoadRobots();
        var newsFlashes = repository.LoadNewsFlashes();
        var items = repository.LoadItems();
        var schedules = repository.LoadNpcSchedules();
        var investments = repository.LoadInvestments();
        var digitalServices = repository.LoadDigitalServices();
        var technicalRepairs = repository.LoadTechnicalRepairs();
        var knotNames = Directory.EnumerateFiles(ResolveRepositoryInkDirectory(), "*.ink")
            .SelectMany(File.ReadLines)
            .Where(static line => line.StartsWith("=== ", StringComparison.Ordinal) && line.EndsWith(" ===", StringComparison.Ordinal))
            .Select(static line => line[4..^4])
            .ToHashSet(StringComparer.Ordinal);

        var act = () => ContentCatalogValidator.Validate(
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
            schedules,
            investments,
            digitalServices,
            technicalRepairs);

        act.Should().NotThrow();
    }

    [Test]
    public async Task Validate_NewsResponseWithUnknownItem_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            NewsFlashes =
            [
                new NewsFlashDefinition
                {
                    Id = "news",
                    Headline = "Headline",
                    Body = "Body",
                    SourceLabel = "Source",
                    MinimumDay = 1,
                    Weight = 1,
                    DurationDays = 2,
                    Responses = [new NewsResponseDefinition { Id = "prepare", Label = "Prepare", Type = NewsResponseType.Prepare, RequiredItemId = "missing", RequiredItemQuantity = 1 }]
                }
            ],
            Items = [new ItemDefinition { Id = "papers", Name = "Papers", Description = "Documents", MaximumQuantity = 1 }]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*invalid required item*");
    }

    [Test]
    public async Task Validate_EmptyBackgrounds_Fails()
    {
        var catalog = BuildValidCatalog() with { Backgrounds = [] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*backgrounds: catalog is empty*");
    }

    [Test]
    public async Task Validate_MissingBackgroundType_Fails()
    {
        var catalog = BuildValidCatalog() with { Backgrounds = [Background(BackgroundType.SudaneseRefugee)] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*missing types MedicalSchoolDropout, ReleasedPoliticalPrisoner*");
    }

    [Test]
    public async Task Validate_BackgroundReferencesMissingKnot_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Backgrounds =
            [
                new Background
                {
                    Type = BackgroundType.MedicalSchoolDropout,
                    Name = "Medical School Dropout",
                    StartingMoney = 50,
                    StartingHealth = 100,
                    StartingEnergy = 80,
                    StartingHunger = 80,
                    StartingStress = 20,
                    MotherStartingHealth = 70,
                    FoodStockpile = 3,
                    InkIntroKnot = "intro_does_not_exist"
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*references missing ink knot 'intro_does_not_exist'*");
    }

    [Test]
    public async Task Validate_DuplicateLocation_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Locations = [.. ValidLocations(), Location(LocationId.Square)]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*duplicate id 'square'*");
    }

    [Test]
    public async Task Validate_MissingLocationId_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Locations = ValidLocations().Where(static location => location.Id != LocationId.Square).ToArray()
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*missing locations square*");
    }

    [Test]
    public async Task Validate_UnknownLocationId_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Locations = [.. ValidLocations(), new Location { Id = new LocationId("moon"), Name = "Moon", District = DistrictId.Dokki }]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*unknown location ids moon*");
    }

    [Test]
    public async Task Validate_UnknownAdvertisedOpportunity_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Locations =
            [
                new Location
                {
                    Id = LocationId.Home,
                    Name = "Home",
                    District = DistrictId.Imbaba,
                    HasJobOpportunities = true,
                    AvailableJobTypes = [(JobType)999]
                },
                .. ValidLocations().Where(static location => location.Id != LocationId.Home)
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*advertises unknown or duplicate job type*");
    }

    [Test]
    public async Task Validate_UncoveredDistrict_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Locations = ValidLocations().Select(static location => new Location
            {
                Id = location.Id,
                Name = location.Name,
                Description = location.Description,
                District = DistrictId.Dokki,
                HasJobOpportunities = location.HasJobOpportunities,
                HasCrimeOpportunities = location.HasCrimeOpportunities,
                HasClinicServices = location.HasClinicServices,
                ClinicVisitBaseCost = location.ClinicVisitBaseCost,
                ClinicOpenDays = location.ClinicOpenDays,
                TravelTimeMinutes = location.TravelTimeMinutes,
                HasCafe = location.HasCafe,
                HasBar = location.HasBar,
                HasBilliards = location.HasBilliards
            }).ToArray()
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*no location covers districts Imbaba, ArdAlLiwa, BulaqAlDakrour, Shubra, DowntownCairo*");
    }

    [Test]
    public async Task Validate_MissingJobType_Fails()
    {
        var catalog = BuildValidCatalog() with { Jobs = [Job(JobType.BakeryWork)] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*jobs: missing types*");
    }

    [Test]
    public async Task Validate_EmptyRandomEvents_Fails()
    {
        var catalog = BuildValidCatalog() with { RandomEvents = [] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*random_events: catalog is empty*");
    }

    [Test]
    public async Task Validate_RandomEventReferencesMissingKnot_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            RandomEvents = [new RandomEvent("test_event", "A test event", new RandomEventEffect { InkKnot = "event_missing" }, 1, 10, null)]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'test_event' references missing ink knot 'event_missing'*");
    }

    [Test]
    public async Task Validate_EmptyDistrictConditions_Fails()
    {
        var catalog = BuildValidCatalog() with { DistrictConditions = [] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*district_conditions: catalog is empty*");
    }

    [Test]
    public async Task Validate_DistrictConditionInvalidPressureRange_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition { Id = "bad_condition", Title = "Bad", Weight = 1, MinDay = 1, MinPolicePressure = 80, MaxPolicePressure = 20 }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'bad_condition' has min police pressure 80 above max 20*");
    }

    [Test]
    public async Task Validate_DistrictConditionReferencesUnknownBoostedEvent_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition
                {
                    Id = "bad_condition",
                    Title = "Bad",
                    Weight = 1,
                    MinDay = 1,
                    Effect = new DistrictConditionEffect { BoostedRandomEventIds = ["not_an_event"] }
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'bad_condition' references unknown boosted random event 'not_an_event'*");
    }

    [Test]
    public async Task Validate_DistrictConditionReferencesUnknownSuppressedEvent_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition
                {
                    Id = "bad_condition",
                    Title = "Bad",
                    Weight = 1,
                    MinDay = 1,
                    Effect = new DistrictConditionEffect { SuppressedRandomEventIds = ["not_an_event"] }
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'bad_condition' references unknown suppressed random event 'not_an_event'*");
    }

    [Test]
    public async Task Validate_DistrictConditionReferencesConfiguredEvent_DoesNotThrow()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition
                {
                    Id = "test_condition",
                    District = DistrictId.Imbaba,
                    Title = "Test",
                    BulletinText = "Test bulletin.",
                    GameplaySummary = "Test summary.",
                    Weight = 1,
                    MinDay = 1,
                    Effect = new DistrictConditionEffect { BoostedRandomEventIds = ["test_event"] }
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().NotThrow();
    }

    [Test]
    public async Task Validate_EmptyPets_Fails()
    {
        var catalog = BuildValidCatalog() with { Pets = [] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*pets: catalog is empty*");
    }

    [Test]
    public async Task Validate_MissingPetType_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Pets = [new PetDefinition { Type = PetType.Cat, Name = "Cat", MaxOwned = 1, OneTimeCost = 50, WeeklyCareCost = 5 }]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*pets: missing types Fish*");
    }

    [Test]
    public async Task Validate_PetReferencesMissingPurchaseLocation_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Pets =
            [
                new PetDefinition { Type = PetType.Cat, Name = "Cat", MaxOwned = 1, PurchaseLocationId = LocationId.Square },
                new PetDefinition { Type = PetType.Fish, Name = "Fish", MaxOwned = 2, PurchaseLocationId = new LocationId("underwater") }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*Fish*references missing purchase location 'underwater'*");
    }

    [Test]
    public async Task Validate_EmptyPlants_Fails()
    {
        var catalog = BuildValidCatalog() with { Plants = [] };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*plants: catalog is empty*");
    }

    [Test]
    public async Task Validate_MissingPlantType_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            Plants = [new PlantDefinition { Type = PlantType.Basil, Name = "Basil", PurchaseLocationId = LocationId.PlantShop }]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*plants: missing types Mint, Parsley*");
    }

    [Test]
    public async Task Validate_NegativePlantHarvestValues_Fail()
    {
        var catalog = BuildValidCatalog() with
        {
            Plants = [.. ValidPlants().Select(plant => plant with { HarvestSalePrice = -5 })]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*plants: * has negative harvest values*");
    }

    [Test]
    public async Task Validate_AggregatesAllProblemsInOneException()
    {
        var catalog = BuildValidCatalog() with
        {
            Backgrounds = [],
            Pets = [],
            Plants = []
        };

        var act = () => Validate(catalog);

        var exception = act.Should().Throw<ContentLoadException>().Which;
        exception.Message.Should().Contain("backgrounds: catalog is empty");
        exception.Message.Should().Contain("pets: catalog is empty");
        exception.Message.Should().Contain("plants: catalog is empty");
    }

    [Test]
    public async Task Validate_DistrictConditionNullEffect_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition { Id = "null_effect_condition", Title = "Bad", Weight = 1, MinDay = 1, Effect = null! }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'null_effect_condition' must provide an effect*");
    }

    [Test]
    public async Task Validate_DistrictConditionNullBoostedEventIds_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition
                {
                    Id = "null_boosted_condition",
                    Title = "Bad",
                    Weight = 1,
                    MinDay = 1,
                    Effect = new DistrictConditionEffect { BoostedRandomEventIds = null! }
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'null_boosted_condition' must provide a boosted random event id list*");
    }

    [Test]
    public async Task Validate_DistrictConditionNullSuppressedEventIds_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition
                {
                    Id = "null_suppressed_condition",
                    Title = "Bad",
                    Weight = 1,
                    MinDay = 1,
                    Effect = new DistrictConditionEffect { SuppressedRandomEventIds = null! }
                }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'null_suppressed_condition' must provide a suppressed random event id list*");
    }

    [Test]
    public async Task Validate_DistrictConditionMissingBulletinText_Fails()
    {
        var catalog = BuildValidCatalog() with
        {
            DistrictConditions =
            [
                new DistrictConditionDefinition { Id = "no_bulletin_condition", Title = "Bad", BulletinText = "", Weight = 1, MinDay = 1 }
            ]
        };

        var act = () => Validate(catalog);

        act.Should().Throw<ContentLoadException>().WithMessage("*'no_bulletin_condition' has no bulletin text*");
    }

    private static void Validate(TestCatalog catalog)
    {
        ContentCatalogValidator.Validate(
            catalog.Backgrounds,
            catalog.Locations,
            catalog.Jobs,
            catalog.RandomEvents,
            catalog.DistrictConditions,
            catalog.Pets,
            catalog.Plants,
            KnownKnots,
            catalog.Robots,
            catalog.NewsFlashes,
            catalog.Items,
            catalog.NpcSchedules,
            catalog.Investments,
            catalog.DigitalServices,
            catalog.TechnicalRepairs);
    }

    private static readonly HashSet<string> KnownKnots = new(StringComparer.Ordinal)
    {
        "intro_MedicalSchoolDropout",
        "intro_ReleasedPoliticalPrisoner",
        "intro_SudaneseRefugee",
        "event_test_scene"
    };

    private static string ResolveRepositoryContentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "data");
            if (File.Exists(Path.Combine(candidate, "backgrounds.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository content/data.");
    }

    private static string ResolveRepositoryInkDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "ink");
            if (File.Exists(Path.Combine(candidate, "main.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository content/ink.");
    }

    private static TestCatalog BuildValidCatalog()
    {
        return new TestCatalog(
            ValidBackgrounds(),
            ValidLocations(),
            ValidJobs(),
            [new RandomEvent("test_event", "A test event", new RandomEventEffect { InkKnot = "event_test_scene" }, 1, 10, null)],
            [new DistrictConditionDefinition { Id = "test_condition", District = DistrictId.Imbaba, Title = "Test", BulletinText = "Test bulletin.", GameplaySummary = "Test summary.", Weight = 1, MinDay = 1 }],
            ValidPets(),
            ValidPlants(),
            ValidRobots(),
            [ValidNewsFlash()],
            [new ItemDefinition { Id = "papers", Name = "Papers", Description = "Documents", MaximumQuantity = 1 }],
            [],
            ValidInvestments(),
            ValidDigitalServices(),
            ValidTechnicalRepairs());
    }

    private sealed record TestCatalog(
        IReadOnlyList<Background> Backgrounds,
        IReadOnlyList<Location> Locations,
        IReadOnlyList<JobShift> Jobs,
        IReadOnlyList<RandomEvent> RandomEvents,
        IReadOnlyList<DistrictConditionDefinition> DistrictConditions,
        IReadOnlyList<PetDefinition> Pets,
        IReadOnlyList<PlantDefinition> Plants,
        IReadOnlyList<RobotDefinition> Robots,
        IReadOnlyList<NewsFlashDefinition> NewsFlashes,
        IReadOnlyList<ItemDefinition> Items,
        IReadOnlyList<NpcScheduleDefinition> NpcSchedules,
        IReadOnlyList<InvestmentDefinition> Investments,
        IReadOnlyList<DigitalServiceActionDefinition> DigitalServices,
        IReadOnlyList<TechnicalRepairActionDefinition> TechnicalRepairs);

    private static InvestmentDefinition[] ValidInvestments()
    {
        return Enum.GetValues<InvestmentType>()
            .Select(type => new InvestmentDefinition
            {
                Type = type,
                Name = type.ToString(),
                Description = "A test investment.",
                Cost = 1,
                WeeklyIncomeMin = 1,
                WeeklyIncomeMax = 2,
                OpportunityLocationId = LocationId.Home,
                RiskProfile = new InvestmentRiskProfile { WeeklyFailureChance = 0.01, BetrayalChance = 0.01 }
            })
            .ToArray();
    }

    private static DigitalServiceActionDefinition[] ValidDigitalServices()
    {
        return Enum.GetValues<DigitalServiceActionType>()
            .Select(type => new DigitalServiceActionDefinition(type, type.ToString(), "A test digital service.", LocationId.Home, 1, 10, 1, 1))
            .ToArray();
    }

    private static TechnicalRepairActionDefinition[] ValidTechnicalRepairs()
    {
        return Enum.GetValues<TechnicalRepairActionType>()
            .Select(type => new TechnicalRepairActionDefinition(type, type.ToString(), "A test repair.", LocationId.Workshop, 1, 10, 1, 1, 1))
            .ToArray();
    }

    private static RobotDefinition[] ValidRobots()
    {
        return Enum.GetValues<RobotType>()
            .Select(type => new RobotDefinition
            {
                Type = type,
                Name = type.ToString(),
                Description = "A test robot.",
                PurchaseCost = 100,
                RepairCost = 10,
                RepairCondition = 40,
                PurchaseLocationId = LocationId.Workshop
            })
            .ToArray();
    }

    private static NewsFlashDefinition ValidNewsFlash()
    {
        return new NewsFlashDefinition
        {
            Id = "valid_news",
            Headline = "Headline",
            Body = "Body",
            SourceLabel = "Source",
            MinimumDay = 1,
            Weight = 1,
            DurationDays = 2
        };
    }

    private static Background[] ValidBackgrounds()
    {
        return Enum.GetValues<BackgroundType>()
            .Select(type => new Background
            {
                Type = type,
                Name = type.ToString(),
                StartingMoney = 50,
                StartingHealth = 100,
                StartingEnergy = 80,
                StartingHunger = 80,
                StartingStress = 20,
                MotherStartingHealth = 70,
                FoodStockpile = 3,
                InkIntroKnot = $"intro_{type}"
            })
            .ToArray();
    }

    private static Location[] ValidLocations()
    {
        return
        [
            Location(LocationId.Home, DistrictId.Imbaba),
            Location(LocationId.Market, DistrictId.Imbaba),
            Location(LocationId.Bakery, DistrictId.Imbaba),
            Location(LocationId.CallCenter, DistrictId.Dokki),
            Location(LocationId.Square, DistrictId.DowntownCairo),
            Location(LocationId.Clinic, DistrictId.ArdAlLiwa),
            Location(LocationId.Workshop, DistrictId.ArdAlLiwa),
            Location(LocationId.Cafe, DistrictId.Dokki),
            Location(LocationId.Pharmacy, DistrictId.BulaqAlDakrour),
            Location(LocationId.Depot, DistrictId.BulaqAlDakrour),
            Location(LocationId.Laundry, DistrictId.Shubra),
            Location(LocationId.FishMarket, DistrictId.Imbaba),
            Location(LocationId.PlantShop, DistrictId.Dokki)
        ];
    }

    private static Location Location(LocationId id, DistrictId district = DistrictId.Dokki)
    {
        return new Location { Id = id, Name = id.Value, District = district, TravelTimeMinutes = 30 };
    }

    private static JobShift[] ValidJobs()
    {
        return Enum.GetValues<JobType>()
            .Select(type => new JobShift { Type = type, Name = type.ToString(), BasePay = 40, EnergyCost = 20, StressCost = 5, DurationMinutes = 480 })
            .ToArray();
    }

    private static JobShift Job(JobType type)
    {
        return new JobShift { Type = type, Name = type.ToString(), BasePay = 40, EnergyCost = 20, StressCost = 5, DurationMinutes = 480 };
    }

    private static PetDefinition[] ValidPets()
    {
        return
        [
            new PetDefinition { Type = PetType.Cat, Name = "Cat", MaxOwned = 1, OneTimeCost = 50, WeeklyCareCost = 5 },
            new PetDefinition { Type = PetType.Fish, Name = "Fish", MaxOwned = 2, OneTimeCost = 80, WeeklyCareCost = 8, PurchaseLocationId = LocationId.FishMarket }
        ];
    }

    private static PlantDefinition[] ValidPlants()
    {
        return Enum.GetValues<PlantType>()
            .Select(type => new PlantDefinition
            {
                Type = type,
                Name = type.ToString(),
                OneTimeCost = 10,
                WeeklyCareCost = 2,
                PurchaseLocationId = LocationId.PlantShop,
                HarvestCycleDays = 5,
                HarvestSalePrice = 10
            })
            .ToArray();
    }

    private static Background Background(BackgroundType type)
    {
        return new Background
        {
            Type = type,
            Name = type.ToString(),
            StartingMoney = 50,
            StartingHealth = 100,
            StartingEnergy = 80,
            StartingHunger = 80,
            StartingStress = 20,
            MotherStartingHealth = 70,
            FoodStockpile = 3,
            InkIntroKnot = $"intro_{type}"
        };
    }
}
