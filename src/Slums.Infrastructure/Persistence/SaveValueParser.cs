namespace Slums.Infrastructure.Persistence;

internal static class SaveValueParser
{
    public static TEnum ParseEnum<TEnum>(string? value, string fieldName)
        where TEnum : struct, Enum
    {
        if (value is null || !Enum.TryParse(value, ignoreCase: false, out TEnum parsed) || !Enum.IsDefined(parsed))
        {
            throw new InvalidDataException($"Save field '{fieldName}' contains unknown value '{value}'.");
        }

        return parsed;
    }
}
