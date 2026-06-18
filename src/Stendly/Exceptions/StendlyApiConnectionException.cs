namespace Stendly.Exceptions;

/// <summary>
/// Raised when the SDK cannot connect to Stendly API.
/// Covers network timeouts, DNS resolution failures, connection refused errors.
/// </summary>
public class StendlyApiConnectionException : StendlyException
{
    /// <summary>
    /// Initializes a new instance of <see cref="StendlyApiConnectionException"/>.
    /// </summary>
    /// <param name="message">Connection error description.</param>
    /// <param name="innerException">Original exception from HttpClient (for debugging).</param>
    public StendlyApiConnectionException(
        string message = "Failed to connect to Stendly API. Check your internet connection.",
        Exception? innerException = null)
        : base(message, innerException, statusCode: null)
    {
    }
}