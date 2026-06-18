using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Complete verified webhook event object.
/// Returned by <c>ConstructEvent</c> after successful signature verification.
/// </summary>
public record WebhookEvent
{
    /// <summary>
    /// Webhook event type (e.g., "payment_intent.succeeded").
    /// </summary>
    [JsonPropertyName("event")]
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Event data containing payment details.
    /// </summary>
    [JsonPropertyName("data")]
    public WebhookData Data { get; init; } = null!;
}