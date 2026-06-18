using System.Text.Json.Serialization;

namespace Stendly.Models;

public enum VerificationStatus
{
    Unverified = 0,
    Pending = 1,
    Verified = 2,
    Rejected = 3,
}

/// <summary>
/// Merchant profile information.
/// Sensitive fields like API keys and webhook secrets are only shown once upon generation.
/// </summary>
public record MerchantProfile
{
    /// <summary>
    /// Merchant unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Merchant business name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// USDC receiving address for receiving payments.
    /// </summary>
    [JsonPropertyName("payoutAddress")]
    public string PayoutAddress { get; init; } = string.Empty;

    /// <summary>
    /// Webhook endpoint URL for payment events.
    /// </summary>
    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; init; }

    /// <summary>
    /// Secret for verifying webhook signatures (prefix: whsec_).
    /// </summary>
    [JsonPropertyName("webhookSecret")]
    public string? WebhookSecret { get; init; }

    /// <summary>
    /// Full API key for API authentication.
    /// ONLY shown once upon generation. Store in secure location!
    /// </summary>
    [JsonPropertyName("rawApiKey")]
    public string? RawApiKey { get; init; }

    /// <summary>
    /// KYB verification status.
    /// </summary>
    [JsonPropertyName("verificationStatus")]
    public VerificationStatus? VerificationStatus { get; init; }

    /// <summary>
    /// Human-readable verification status label.
    /// </summary>
    [JsonIgnore]
    public string VerificationStatusLabel =>
        VerificationStatus.HasValue
            ? VerificationStatus.Value.ToString()
            : "Unknown";
}