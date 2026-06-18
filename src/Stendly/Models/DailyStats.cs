using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Daily statistics entry for chart data.
/// </summary>
public record DailyStats
{
    /// <summary>
    /// Date for this statistics entry.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; init; }

    /// <summary>
    /// Total volume in cents for this day.
    /// </summary>
    [JsonPropertyName("volumeCents")]
    public int VolumeCents { get; init; }

    /// <summary>
    /// Number of transactions for this day.
    /// </summary>
    [JsonPropertyName("transactions")]
    public int Transactions { get; init; }
}