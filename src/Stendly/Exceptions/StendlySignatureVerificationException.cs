namespace Stendly.Exceptions;

/// <summary>
/// Raised when webhook signature verification fails.
/// Indicates that the webhook payload signature is invalid or the timestamp is too old.
/// </summary>
public class StendlySignatureVerificationException : StendlyException
{
    /// <summary>
    /// Specific reason for verification failure.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlySignatureVerificationException"/>.
    /// </summary>
    /// <param name="message">Error description.</param>
    /// <param name="reason">Specific reason (e.g., "signature_mismatch", "timestamp_expired").</param>
    public StendlySignatureVerificationException(
        string message = "Webhook signature verification failed.",
        string? reason = null)
        : base(BuildMessage(message, reason))
    {
        Reason = reason;
    }

    private static string BuildMessage(string message, string? reason)
    {
        if (reason != null)
            return $"{message} Reason: {reason}";
        return message;
    }
}