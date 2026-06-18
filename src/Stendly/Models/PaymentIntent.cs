using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Payment intent object representing a pending or completed payment.
/// Contains the payment reference and merchant destination address for the USDC transfer on Solana.
/// </summary>
public record PaymentIntent
{
    /// <summary>
    /// Unique identifier for the payment intent.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Merchant's order reference (must be unique per merchant).
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// Amount requested in cents (e.g., $50.00 = 5000).
    /// </summary>
    [JsonPropertyName("expectedAmountCents")]
    public int ExpectedAmountCents { get; init; }

    /// <summary>
    /// Amount actually paid in cents (0 if not yet paid).
    /// </summary>
    [JsonPropertyName("paidAmountCents")]
    public int PaidAmountCents { get; init; }

    /// <summary>
    /// Generated Solana reference address used to correlate the payment.
    /// </summary>
    [JsonPropertyName("referenceAddress")]
    public string ReferenceAddress { get; init; } = string.Empty;

    /// <summary>
    /// Merchant's USDC receiving address.
    /// </summary>
    [JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; init; } = string.Empty;

    /// <summary>
    /// Current status of the payment intent.
    /// </summary>
    [JsonPropertyName("status")]
    public PaymentIntentStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when the intent expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; init; }
}
