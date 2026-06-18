using Stendly.Clients;

namespace Stendly;

/// <summary>
/// Main interface for the Stendly .NET SDK.
/// Provides access to all API operations through typed sub-clients.
/// </summary>
public interface IStendlyClient
{
    /// <summary>
    /// Payment intents operations.
    /// </summary>
    IIntentsClient Intents { get; }

    /// <summary>
    /// POS terminals operations.
    /// </summary>
    ITerminalsClient Terminals { get; }

    /// <summary>
    /// Webhook management and signature verification.
    /// </summary>
    IWebhooksClient Webhooks { get; }

    /// <summary>
    /// Merchant account management and analytics.
    /// </summary>
    IMerchantClient Merchant { get; }
}