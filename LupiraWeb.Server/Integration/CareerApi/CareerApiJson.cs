using System.Text.Json;
using System.Text.Json.Serialization;

namespace LupiraWeb.Server.Integration.CareerApi;

internal static class CareerApiJson
{
    /// <summary>
    /// Deserialization options for CareerApi responses. Web defaults give camelCase, case-insensitive
    /// property matching; the string-enum converter reads both string and numeric enum encodings, so we
    /// are robust whether CareerApi serializes enums as names or numbers.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
