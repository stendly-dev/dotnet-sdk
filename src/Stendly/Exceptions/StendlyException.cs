namespace Stendly.Exceptions;

/// <summary>
/// Base exception for all Stendly SDK errors.
/// All other SDK-specific exceptions inherit from this class.
/// </summary>
public class StendlyException : Exception
{
    /// <summary>
    /// HTTP status code returned by the API (if applicable).
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Unique request identifier for debugging (if available).
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyException"/>.
    /// </summary>
    /// <param name="message">Human-readable error description.</param>
    /// <param name="statusCode">HTTP status code returned by the API.</param>
    /// <param name="requestId">Request ID from response headers for support queries.</param>
    public StendlyException(string message, int? statusCode = null, string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyException"/> with an inner exception.
    /// </summary>
    /// <param name="message">Human-readable error description.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    /// <param name="statusCode">HTTP status code returned by the API.</param>
    /// <param name="requestId">Request ID from response headers for support queries.</param>
    public StendlyException(string message, Exception? innerException, int? statusCode = null, string? requestId = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }

    /// <summary>
    /// Returns string representation with status code if available.
    /// </summary>
    public override string ToString()
    {
        if (StatusCode.HasValue)
            return $"[{StatusCode}] {Message}";
        return Message;
    }
}