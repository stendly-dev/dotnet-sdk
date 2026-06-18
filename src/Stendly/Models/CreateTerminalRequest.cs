using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Request model for creating a POS terminal.
/// </summary>
public record CreateTerminalRequest
{
    /// <summary>
    /// Terminal display name (max 100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}