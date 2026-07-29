using System.Linq;
using System.Text.Json.Nodes;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Ai.CriteriaExtraction;

/// <summary>
/// JSON Schema for the structured OpenRouter response (response_format: json_schema, strict).
/// Field names here are the wire contract with the LLM; <see cref="CriteriaSanitizer"/> maps them
/// onto the locked <see cref="VehicleSearchCriteria"/> shape.
/// </summary>
public static class CriteriaJsonSchema
{
    public const string Name = "vehicle_search_criteria";

    public static readonly string Schema = Build().ToJsonString();

    private static JsonObject Build()
    {
        var properties = new JsonObject
        {
            ["make"] = TypeOnly("string"),
            ["model"] = TypeOnly("string"),
            ["yearFrom"] = TypeOnly("integer"),
            ["yearTo"] = TypeOnly("integer"),
            ["priceFrom"] = TypeOnly("number"),
            ["priceTo"] = TypeOnly("number"),
            ["mileageMax"] = TypeOnly("integer"),
            ["fuelType"] = EnumOrNull(Enum.GetNames<FuelType>()),
            ["transmission"] = EnumOrNull(Enum.GetNames<TransmissionType>()),
            ["bodyType"] = EnumOrNull(Enum.GetNames<BodyType>()),
            ["enginePowerHpFrom"] = TypeOnly("integer"),
            ["enginePowerHpTo"] = TypeOnly("integer"),
            ["seatsMin"] = TypeOnly("integer"),
            ["excludeDamaged"] = TypeOnly("boolean"),
            ["locationContains"] = TypeOnly("string"),
            ["keywords"] = StringArray(),
            ["softPreferences"] = StringArray(),
            ["sortBy"] = EnumOrNull(Enum.GetNames<VehicleSortBy>()),
            ["limit"] = TypeOnly("integer"),
            ["clarificationQuestion"] = TypeOnly("string"),
            ["intro"] = TypeOnly("string")
        };

        var required = new JsonArray();
        foreach (var key in properties.Select(p => p.Key))
        {
            required.Add(key);
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }

    private static JsonObject TypeOnly(string type) => new()
    {
        ["type"] = new JsonArray { type, "null" }
    };

    private static JsonObject StringArray() => new()
    {
        ["type"] = new JsonArray { "array", "null" },
        ["items"] = new JsonObject { ["type"] = "string" }
    };

    private static JsonObject EnumOrNull(IEnumerable<string> names)
    {
        var values = new JsonArray();
        foreach (var name in names)
        {
            values.Add(name);
        }
        values.Add(null);

        return new JsonObject
        {
            ["type"] = new JsonArray { "string", "null" },
            ["enum"] = values
        };
    }
}
