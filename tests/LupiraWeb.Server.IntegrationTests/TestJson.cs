using System.Text.Json;
using System.Text.Json.Serialization;

namespace LupiraWeb.Server.Tests;

/// <summary>
/// Deserialization options matching the API's wire contract: web defaults plus the string-enum converter,
/// so responses serialized with enum names (see Program.cs) read back into the strongly-typed DTOs.
/// </summary>
internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
