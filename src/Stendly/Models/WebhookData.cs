using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Webhook event payload data containing payment details.
/// </summary>
public record WebhookData
{
    /// <summary>
    /// Payment intent UUID that triggered this event.
    /// </summary>
    [JsonPropertyName("paymentIntentId")]
    public Guid PaymentIntentId { get; init; }

    /// <summary>
    /// Merchant's order reference.
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// Actual amount in cents (may differ from expected).
    /// </summary>
    [JsonPropertyName("amountCents")]
    public int AmountCents { get; init; }

    /// <summary>
    /// Originally expected payment amount in cents.
    /// </summary>
    [JsonPropertyName("expectedAmountCents")]
    public int ExpectedAmountCents { get; init; }

    /// <summary>
    /// Solana transaction signature (base-58 encoded), if payment completed.
    /// </summary>
    [JsonPropertyName("txSignature")]
    public string? TxSignature { get; init; }
}