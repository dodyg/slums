using System.Text.Json.Serialization;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.World;
using Slums.Core.Robotics;
using Slums.Core.Inventory;
using Slums.Core.Relationships;
using Slums.Core.World.News;

namespace Slums.Infrastructure.Content;

[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
[JsonSerializable(typeof(List<Background>))]
[JsonSerializable(typeof(List<Location>))]
[JsonSerializable(typeof(List<JobShift>))]
[JsonSerializable(typeof(List<RandomEventDefinition>))]
[JsonSerializable(typeof(List<DistrictConditionDefinition>))]
[JsonSerializable(typeof(List<PetDefinition>))]
[JsonSerializable(typeof(List<PlantDefinition>))]
[JsonSerializable(typeof(List<RobotDefinition>))]
[JsonSerializable(typeof(List<NewsFlashDefinition>))]
[JsonSerializable(typeof(List<ItemDefinition>))]
[JsonSerializable(typeof(List<NpcScheduleDefinition>))]
internal sealed partial class ContentJsonContext : JsonSerializerContext
{
}
