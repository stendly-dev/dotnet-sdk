using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Request model for creating a payment intent.
/// </summary>
public record CreatePaymentIntentRequest
{
    /// <summary>
    /// Amount to charge in cents (e.g., 4999 = $49.99). Must be positive.
    /// </summary>
    [JsonPropertyName("amountCents")]
    public int AmountCents { get; init; }

    /// <summary>
    /// Unique order reference (max 100 characters).
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// Optional terminal ID for POS/payment scenarios.
    /// </summary>
    [JsonPropertyName("terminalId")]
    public Guid? TerminalId { get; init; }
}