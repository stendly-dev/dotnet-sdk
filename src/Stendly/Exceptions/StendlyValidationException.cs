namespace Stendly.Exceptions;

/// <summary>
/// Raised when request validation fails (HTTP 400).
/// Contains detailed information about which fields failed validation.
/// </summary>
public class StendlyValidationException : StendlyException
{
    /// <summary>
    /// Specific field that failed validation (if available).
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Additional error details from API response.
    /// </summary>
    public IReadOnlyDictionary<string, string> Details { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyValidationException"/>.
    /// </summary>
    /// <param name="message">Validation error description.</param>
    /// <param name="field">Name of the field that failed validation.</param>
    /// <param name="details">Additional error details from the API.</param>
    /// <param name="statusCode">HTTP status code (always 400).</param>
    /// <param name="requestId">Request ID for support.</param>
    public StendlyValidationException(
        string message,
        string? field = null,
        Dictionary<string, string>? details = null,
        int statusCode = 400,
        string? requestId = null)
        : base(message, statusCode, requestId)
    {
        Field = field;
        Details = details?.AsReadOnly() ?? new Dictionary<string, string>().AsReadOnly();
    }

    /// <summary>
    /// Returns string representation with field info if available.
    /// </summary>
    public override string ToString()
    {
        var baseStr = base.ToString();
        if (Field != null)
            return $"{baseStr} (field: {Field})";
        return baseStr;
    }
}