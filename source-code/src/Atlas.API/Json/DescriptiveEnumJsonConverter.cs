using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atlas.API.Json;

/// <summary>
/// Replaces the default JsonStringEnumConverter with one that produces a human-readable error
/// when an unknown enum value is received, listing the valid options in the response.
/// </summary>
public sealed class DescriptiveEnumJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(DescriptiveEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

file sealed class DescriptiveEnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private static readonly string ValidValues = string.Join(", ", Enum.GetNames<T>());

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected a string value for {typeof(T).Name}. Valid values: {ValidValues}.");

        var value = reader.GetString();

        if (Enum.TryParse<T>(value, ignoreCase: false, out var result))
            return result;

        throw new JsonException(
            $"Invalid value '{value}' for {typeof(T).Name}. Valid values: {ValidValues}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
