namespace Stendly.Exceptions;

/// <summary>
/// Raised when API rate limit is exceeded (HTTP 429).
/// Provides the Retry-After value so developers can implement proper backoff.
/// </summary>
public class StendlyRateLimitException : StendlyException
{
    /// <summary>
    /// Number of seconds to wait before retrying.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyRateLimitException"/>.
    /// </summary>
    /// <param name="message">Rate limit error message.</param>
    /// <param name="retryAfterSeconds">Seconds to wait before retrying (from Retry-After header).</param>
    /// <param name="statusCode">HTTP status code (always 429).</param>
    /// <param name="requestId">Request ID for support.</param>
    public StendlyRateLimitException(
        string message = "Rate limit exceeded. Please slow down your requests.",
        int? retryAfterSeconds = null,
        int statusCode = 429,
        string? requestId = null)
        : base(BuildMessage(message, retryAfterSeconds), statusCode, requestId)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    private static string BuildMessage(string message, int? retryAfterSeconds)
    {
        if (retryAfterSeconds.HasValue)
            return $"{message} Retry after {retryAfterSeconds.Value} seconds.";
        return message;
    }
}