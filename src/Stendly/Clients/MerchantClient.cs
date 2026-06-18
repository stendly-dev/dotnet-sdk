using Stendly.Infrastructure;
using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Client for merchant account management and analytics.
/// </summary>
internal sealed class MerchantClient : IMerchantClient
{
    private readonly StendlyHttpClient _http;

    /// <summary>
    /// Initializes a new instance of <see cref="MerchantClient"/>.
    /// </summary>
    /// <param name="http">The HTTP client wrapper.</param>
    public MerchantClient(StendlyHttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <inheritdoc />
    public async Task<MerchantProfile> GetProfileAsync(
        CancellationToken cancellationToken = default)
    {
        return await _http.SendAsync<MerchantProfile>(
            HttpMethod.Get,
            "/api/b2b/merchants/me",
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MerchantStats> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _http.SendAsync<MerchantStats>(
            HttpMethod.Get,
            "/api/b2b/merchants/stats",
            cancellationToken: cancellationToken);
    }
}