using Stendly.Clients;
using Stendly.Exceptions;
using Stendly.Infrastructure;

namespace Stendly;

/// <summary>
/// Main client for the Stendly .NET SDK.
/// 
/// Provides access to all API operations through typed sub-clients:
/// - <see cref="Intents"/> — payment intents
/// - <see cref="Terminals"/> — POS terminals
/// - <see cref="Webhooks"/> — webhook management and verification
/// - <see cref="Merchant"/> — merchant account and analytics
/// 
/// Example usage (DI with IHttpClientFactory):
/// <code>
/// services.AddHttpClient(c =>
/// {
///     return new StendlyClient(c, "st_live_your_api_key");
/// });
/// 
/// // Or direct instantiation
/// var stendly = new StendlyClient(new HttpClient(), "st_live_your_api_key");
/// var intent = await stendly.Intents.CreateIntentAsync(amountCents: 4999, orderId: "order_001");
/// </code>
/// CRITICAL: This class does NOT create its own HttpClient. It accepts one via constructor
/// to properly support IHttpClientFactory and Dependency Injection patterns.
/// </summary>
public sealed class StendlyClient : IStendlyClient, IDisposable
{
    private readonly StendlyHttpClient _http;

    /// <summary>
    /// Gets the base URL for the API.
    /// </summary>
    internal static class DefaultUrls
    {
        /// <summary>
        /// Production API base URL.
        /// </summary>
        public const string Mainnet = "https://api.stendly.com";

        /// <summary>
        /// Development/sandbox API base URL.
        /// </summary>
        public const string Devnet = "https://api-devnet.stendly.com";
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyClient"/>.
    /// 
    /// IMPORTANT: The provided HttpClient should have its BaseAddress set appropriately.
    /// If not set, defaults to mainnet (https://api.stendly.com).
    /// </summary>
    /// <param name="httpClient">
    /// The HttpClient instance. In DI scenarios, this comes from IHttpClientFactory.
    /// The client is NOT disposed by this class — caller owns its lifetime.
    /// </param>
    /// <param name="apiKey">
    /// Merchant API key. Must start with "st_live_".
    /// Use environment="devnet" for development/testing.
    /// </param>
    /// <param name="environment">
    /// API environment: "mainnet" (production) or "devnet" (sandbox).
    /// Auto-detected from apiKey if not specified.
    /// </param>
    /// <param name="maxRetries">
    /// Maximum retry attempts for transient failures (default: 2).
    /// </param>
    /// <exception cref="ArgumentNullException">If httpClient or apiKey is null.</exception>
    /// <exception cref="ArgumentException">If apiKey format is invalid.</exception>
    public StendlyClient(
        HttpClient httpClient,
        string apiKey,
        string environment = "mainnet",
        int maxRetries = 2)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        // Validate API key format
        if (!apiKey.StartsWith("st_live_", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Invalid API key format. Must start with 'st_live_'.",
                nameof(apiKey));
        }

        // Set BaseAddress if not already set
        if (httpClient.BaseAddress == null)
        {
            httpClient.BaseAddress = new Uri(
                environment == "devnet" ? DefaultUrls.Devnet : DefaultUrls.Mainnet);
        }

        Environment = environment;
        _http = new StendlyHttpClient(httpClient, apiKey, maxRetries);

        // Initialize sub-clients
        Intents = new IntentsClient(_http);
        Terminals = new TerminalsClient(_http);
        Webhooks = new WebhooksClient(_http);
        Merchant = new MerchantClient(_http);
    }

    /// <summary>
    /// Gets the configured environment name.
    /// </summary>
    public string Environment { get; }

    /// <summary>
    /// Build a public checkout URL for a payment intent.
    /// </summary>
    /// <param name="intentId">Payment intent UUID.</param>
    /// <returns>Full URL to the checkout page.</returns>
    public string InvoiceUrl(string intentId)
    {
        var appUrl = Environment == "devnet"
            ? "https://app-devnet.stendly.com"
            : "https://app.stendly.com";
        return $"{appUrl}/checkout?invoice={intentId}";
    }

    /// <inheritdoc />
    public IIntentsClient Intents { get; }

    /// <inheritdoc />
    public ITerminalsClient Terminals { get; }

    /// <inheritdoc />
    public IWebhooksClient Webhooks { get; }

    /// <inheritdoc />
    public IMerchantClient Merchant { get; }

    /// <summary>
    /// Disposes the internal StendlyHttpClient wrapper.
    /// The original HttpClient passed to the constructor is NOT disposed.
    /// </summary>
    public void Dispose()
    {
        _http.Dispose();
    }
}
