using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Stendly.Exceptions;
using Stendly.Infrastructure;
using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Client for webhook management and signature verification.
/// </summary>
internal sealed class WebhooksClient : IWebhooksClient
{
    private readonly StendlyHttpClient _http;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhooksClient"/>.
    /// </summary>
    /// <param name="http">The HTTP client wrapper.</param>
    public WebhooksClient(StendlyHttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <inheritdoc />
    public async Task<bool> UpdateWebhookUrlAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new StendlyValidationException("Webhook URL is required.", field: nameof(url));

        var request = new UpdateWebhookRequest { WebhookUrl = url };

        using var response = await _http.SendRawAsync(
            HttpMethod.Patch,
            "/api/b2b/merchants/webhook",
            body: request,
            cancellationToken: cancellationToken);

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// CRITICAL: Uses <c>CryptographicOperations.FixedTimeEquals</c> for constant-time comparison
    /// to prevent timing attacks. Never use == or String.Equals for signature comparison.
    /// </remarks>
    public Task<WebhookEvent> ConstructEventAsync(
        byte[] payload,
        string signatureHeader,
        string webhookSecret,
        int toleranceSeconds = 300,
        CancellationToken cancellationToken = default)
    {
        // This is a CPU-bound operation — no I/O involved.
        // The method is async to match the interface, but execution is synchronous.
        cancellationToken.ThrowIfCancellationRequested();

        if (payload == null || payload.Length == 0)
            throw new StendlySignatureVerificationException(
                "Payload is empty.", reason: "empty_payload");

        if (string.IsNullOrWhiteSpace(signatureHeader))
            throw new StendlySignatureVerificationException(
                "Missing X-Stendly-Signature header.", reason: "missing_signature");

        if (string.IsNullOrWhiteSpace(webhookSecret))
            throw new StendlySignatureVerificationException(
                "Webhook secret is required.", reason: "missing_secret");

        // Parse signature header: "t={timestamp},v1={hash}"
        var (timestamp, signatures) = ParseSignatureHeader(signatureHeader);

        // Verify timestamp (prevent replay attacks)
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var age = currentTime - timestamp;

        if (age > toleranceSeconds)
            throw new StendlySignatureVerificationException(
                $"Webhook is too old (age: {age}s, max: {toleranceSeconds}s)",
                reason: "timestamp_expired");

        if (timestamp > currentTime + 30) // 30 sec clock skew tolerance
            throw new StendlySignatureVerificationException(
                "Webhook timestamp is in the future",
                reason: "future_timestamp");

        // Compute expected signature
        var expectedSignature = ComputeSignature(webhookSecret, timestamp, payload);

        // CRITICAL: Constant-time comparison to prevent timing attacks
        // Do NOT use == or String.Equals for signature comparison
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);

        foreach (var providedSig in signatures)
        {
            var providedBytes = Encoding.UTF8.GetBytes(providedSig);

            if (providedBytes.Length == expectedBytes.Length &&
                CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
            {
                // Signature verified — parse and return event
                var payloadString = Encoding.UTF8.GetString(payload);
                var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(
                    payloadString,
                    JsonSerializerOptionsFactory.Default);

                if (webhookEvent == null)
                    throw new StendlySignatureVerificationException(
                        "Invalid JSON payload", reason: "invalid_json");

                return Task.FromResult(webhookEvent);
            }
        }

        // No signature matched
        throw new StendlySignatureVerificationException(
            "Signature verification failed", reason: "signature_mismatch");
    }

    /// <summary>
    /// Parses the X-Stendly-Signature header.
    /// Format: "t=timestamp,v1=hash1,v2=hash2"
    /// </summary>
    private static (long Timestamp, string[] Signatures) ParseSignatureHeader(string header)
    {
        var parts = header.Split(',');
        long? timestamp = null;
        var signatures = new List<string>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var eqIndex = trimmed.IndexOf('=');
            if (eqIndex < 0)
                continue;

            var key = trimmed[..eqIndex];
            var value = trimmed[(eqIndex + 1)..];

            if (key == "t")
            {
                if (long.TryParse(value, out var ts))
                    timestamp = ts;
            }
            else if (key is "v1" or "v2")
            {
                signatures.Add(value);
            }
        }

        if (!timestamp.HasValue || signatures.Count == 0)
            throw new StendlySignatureVerificationException(
                "Invalid signature header format", reason: "invalid_header_format");

        return (timestamp.Value, signatures.ToArray());
    }

    /// <summary>
    /// Computes the expected HMAC-SHA256 signature.
    /// Signature = HMAC-SHA256(secret, timestamp + payload)
    /// </summary>
    private static string ComputeSignature(string secret, long timestamp, byte[] payload)
    {
        var timestampBytes = Encoding.UTF8.GetBytes(timestamp.ToString());

        // message = timestamp + payload
        var message = new byte[timestampBytes.Length + payload.Length];
        Buffer.BlockCopy(timestampBytes, 0, message, 0, timestampBytes.Length);
        Buffer.BlockCopy(payload, 0, message, timestampBytes.Length, payload.Length);

        var secretBytes = Encoding.UTF8.GetBytes(secret);

        var hmac = HMACSHA256.HashData(secretBytes, message);
        return Convert.ToHexStringLower(hmac);
    }
}