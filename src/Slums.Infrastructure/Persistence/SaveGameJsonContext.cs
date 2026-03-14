using System.Text.Json.Serialization;
using Slums.Core.Investments;

namespace Slums.Infrastructure.Persistence;

[JsonSourceGenerationOptions(UseStringEnumConverter = true, WriteIndented = true)]
[JsonSerializable(typeof(GameSessionSaveDocument))]
[JsonSerializable(typeof(GameSessionSnapshot))]
[JsonSerializable(typeof(GameSessionClockSnapshot))]
[JsonSerializable(typeof(GameSessionPlayerSnapshot))]
[JsonSerializable(typeof(GameSessionWorldSnapshot))]
[JsonSerializable(typeof(ActiveDistrictConditionSnapshot))]
[JsonSerializable(typeof(List<ActiveDistrictConditionSnapshot>))]
[JsonSerializable(typeof(GameSessionRelationshipSnapshot))]
[JsonSerializable(typeof(GameSessionNpcRelationshipSnapshot))]
[JsonSerializable(typeof(GameSessionJobProgressSnapshot))]
[JsonSerializable(typeof(GameSessionJobTrackSnapshot))]
[JsonSerializable(typeof(GameSessionCrimeSnapshot))]
[JsonSerializable(typeof(GameSessionWorkSnapshot))]
[JsonSerializable(typeof(GameSessionRunSnapshot))]
[JsonSerializable(typeof(GameSessionNarrativeSnapshot))]
[JsonSerializable(typeof(NarrativeProgressSnapshot))]
[JsonSerializable(typeof(InvestmentSnapshot))]
[JsonSerializable(typeof(List<InvestmentSnapshot>))]
internal sealed partial class SaveGameJsonContext : JsonSerializerContext
{
}
