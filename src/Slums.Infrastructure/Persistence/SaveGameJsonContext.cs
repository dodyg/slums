using System.Text.Json.Serialization;

namespace Slums.Infrastructure.Persistence;

[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
[JsonSerializable(typeof(SaveEnvelope))]
[JsonSerializable(typeof(GameStateDto))]
[JsonSerializable(typeof(NarrativeStateDto))]
internal sealed partial class SaveGameJsonContext : JsonSerializerContext
{
}