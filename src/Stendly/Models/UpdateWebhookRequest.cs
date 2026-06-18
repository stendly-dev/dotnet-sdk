using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Request model for updating webhook URL.
/// </summary>
public record UpdateWebhookRequest
{
    /// <summary>
    /// New webhook endpoint URL (must be HTTPS in production).
    /// </summary>
    [JsonPropertyName("webhookUrl")]
    public string WebhookUrl { get; init; } = string.Empty;
}