using System.Text.Json;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Serialization;

public sealed class JsonSerializerService
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, value?.GetType() ?? typeof(T), DefaultOptions);

    public T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public object? Deserialize(string json, Type returnType)
    {
        ArgumentNullException.ThrowIfNull(returnType);

        return JsonSerializer.Deserialize(json, returnType, DefaultOptions);
    }
}
