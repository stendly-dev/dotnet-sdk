using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// POS terminal object for in-person payments.
/// </summary>
public record Terminal
{
    /// <summary>
    /// Unique terminal identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Terminal display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Whether the terminal is active and accepting payments.
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    /// <summary>
    /// Terminal creation timestamp.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }
}