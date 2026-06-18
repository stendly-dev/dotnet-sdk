using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Interface for merchant account management and analytics.
/// </summary>
public interface IMerchantClient
{
    /// <summary>
    /// Retrieves the current merchant profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Merchant profile with payout address and webhook configuration.</returns>
    Task<MerchantProfile> GetProfileAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves merchant statistics for the last 30 days.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Statistics including volume, transactions, and daily breakdown.</returns>
    Task<MerchantStats> GetStatsAsync(
        CancellationToken cancellationToken = default);
}