using System.Text.Json;

namespace Stendly.Infrastructure;

/// <summary>
/// Provides configured <see cref="JsonSerializerOptions"/> instances for the SDK.
/// All options use camelCase naming policy for API compatibility.
/// </summary>
internal static class JsonSerializerOptionsFactory
{
    /// <summary>
    /// Gets the default serializer options with camelCase naming policy.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };
}