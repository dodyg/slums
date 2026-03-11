using Slums.Core.Characters;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.World;

namespace Slums.Application.Content;

public interface IContentRepository
{
    public IReadOnlyList<Background> LoadBackgrounds();

    public IReadOnlyList<Location> LoadLocations();

    public IReadOnlyList<JobShift> LoadJobs();

    public IReadOnlyList<RandomEvent> LoadRandomEvents();
}