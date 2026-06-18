namespace Stendly.Exceptions;

/// <summary>
/// Raised when API authentication fails (HTTP 401 or 403).
/// Indicates that the provided API key is invalid, expired, or lacks permissions.
/// </summary>
public class StendlyAuthenticationException : StendlyException
{
    /// <summary>
    /// Initializes a new instance of <see cref="StendlyAuthenticationException"/>.
    /// </summary>
    /// <param name="message">Detailed authentication error message.</param>
    /// <param name="statusCode">HTTP status code (always 401 or 403).</param>
    /// <param name="requestId">Request ID for support.</param>
    public StendlyAuthenticationException(
        string message = "Authentication failed. Check your API key.",
        int statusCode = 401,
        string? requestId = null)
        : base(message, statusCode, requestId)
    {
    }
}