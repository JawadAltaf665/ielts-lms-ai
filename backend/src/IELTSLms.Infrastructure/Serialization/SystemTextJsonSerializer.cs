using System.Text.Json;
using System.Text.Json.Serialization;
using IELTSLms.Abstractions.Serialization;

namespace IELTSLms.Infrastructure.Serialization;

internal sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
