using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Interface for webhook management and signature verification.
/// </summary>
public interface IWebhooksClient
{
    /// <summary>
    /// Updates the webhook URL for payment notifications.
    /// </summary>
    /// <param name="url">Webhook endpoint URL (must be HTTPS in production).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if webhook URL was successfully updated.</returns>
    Task<bool> UpdateWebhookUrlAsync(
        string url,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies webhook signature and constructs an event object.
    /// Uses constant-time comparison via <c>CryptographicOperations.FixedTimeEquals</c>.
    /// </summary>
    /// <param name="payload">Raw webhook request body bytes.</param>
    /// <param name="signatureHeader">Value of X-Stendly-Signature header (format: "t={timestamp},v1={hash}").</param>
    /// <param name="webhookSecret">Webhook secret from merchant profile (starts with "whsec_").</param>
    /// <param name="toleranceSeconds">Maximum age of webhook in seconds (default: 300).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verified webhook event with structured data.</returns>
    Task<WebhookEvent> ConstructEventAsync(
        byte[] payload,
        string signatureHeader,
        string webhookSecret,
        int toleranceSeconds = 300,
        CancellationToken cancellationToken = default);
}