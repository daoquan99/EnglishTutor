using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public static class ApiJsonOptions
{
    public static JsonSerializerOptions Default { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
