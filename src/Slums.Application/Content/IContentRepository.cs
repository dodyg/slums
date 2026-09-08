using Slums.Core.Characters;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.World;
using Slums.Core.Robotics;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Core.World.News;
using Slums.Core.Investments;
using Slums.Core.Technology;

namespace Slums.Application.Content;

public interface IContentRepository
{
    public IReadOnlyList<Background> LoadBackgrounds();

    public IReadOnlyList<Location> LoadLocations();

    public IReadOnlyList<JobShift> LoadJobs();

    public IReadOnlyList<RandomEvent> LoadRandomEvents();

    public IReadOnlyList<DistrictConditionDefinition> LoadDistrictConditions();

    public IReadOnlyList<PetDefinition> LoadPets();

    public IReadOnlyList<PlantDefinition> LoadPlants();

    public IReadOnlyList<RobotDefinition> LoadRobots();

    public IReadOnlyList<NewsFlashDefinition> LoadNewsFlashes();

    public IReadOnlyList<ItemDefinition> LoadItems();

    public IReadOnlyList<NpcScheduleDefinition> LoadNpcSchedules();

    public IReadOnlyList<InvestmentDefinition> LoadInvestments();

    public IReadOnlyList<DigitalServiceActionDefinition> LoadDigitalServices();

    public IReadOnlyList<TechnicalRepairActionDefinition> LoadTechnicalRepairs();
}
