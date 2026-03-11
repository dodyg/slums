using System.Text.Json.Serialization;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.World;

namespace Slums.Infrastructure.Content;

[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
[JsonSerializable(typeof(List<Background>))]
[JsonSerializable(typeof(List<Location>))]
[JsonSerializable(typeof(List<JobShift>))]
[JsonSerializable(typeof(List<RandomEventDefinition>))]
internal sealed partial class ContentJsonContext : JsonSerializerContext
{
}