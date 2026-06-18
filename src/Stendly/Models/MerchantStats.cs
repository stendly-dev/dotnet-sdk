using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Merchant analytics and statistics (30-day period).
/// </summary>
public record MerchantStats
{
    /// <summary>
    /// Total payment volume in cents over the last 30 days.
    /// </summary>
    [JsonPropertyName("totalVolumeCents")]
    public int TotalVolumeCents { get; init; }

    /// <summary>
    /// Total transaction count (last 30 days).
    /// </summary>
    [JsonPropertyName("totalTransactions")]
    public int TotalTransactions { get; init; }

    /// <summary>
    /// Number of successfully paid transactions.
    /// </summary>
    [JsonPropertyName("successfulTransactions")]
    public int SuccessfulTransactions { get; init; }

    /// <summary>
    /// Daily statistics for last 31 days (today + 30 days prior).
    /// </summary>
    [JsonPropertyName("chartData")]
    public IReadOnlyList<DailyStats> ChartData { get; init; } = Array.Empty<DailyStats>();

    /// <summary>
    /// Calculate payment success rate as percentage (0-100).
    /// </summary>
    [JsonIgnore]
    public double SuccessRate =>
        TotalTransactions == 0 ? 0.0 : (double)SuccessfulTransactions / TotalTransactions * 100;

    /// <summary>
    /// Calculate average transaction amount in cents.
    /// </summary>
    [JsonIgnore]
    public double AverageTransactionCents =>
        SuccessfulTransactions == 0 ? 0.0 : (double)TotalVolumeCents / SuccessfulTransactions;
}