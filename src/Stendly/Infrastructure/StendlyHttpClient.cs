using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Stendly.Exceptions;

namespace Stendly.Infrastructure;

/// <summary>
/// Internal HTTP client wrapper for Stendly API communication.
/// Handles authentication, retries, error parsing, and idempotency keys.
/// </summary>
internal sealed class StendlyHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly int _maxRetries;
    private readonly ProductInfoHeaderValue _userAgent;
    private static readonly Random _jitter = new();

    /// <summary>
    /// Initializes a new instance of <see cref="StendlyHttpClient"/>.
    /// Does NOT create its own HttpClient — receives it via constructor
    /// to support IHttpClientFactory and DI scenarios.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance (from DI or IHttpClientFactory).</param>
    /// <param name="apiKey">Merchant API key (st_live_*).</param>
    /// <param name="maxRetries">Maximum retry attempts for transient failures.</param>
    public StendlyHttpClient(HttpClient httpClient, string apiKey, int maxRetries = 2)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _maxRetries = maxRetries;

        _userAgent = new ProductInfoHeaderValue("dotnet-sdk", "0.1.0");
    }

    /// <summary>
    /// Sends an HTTP request and deserializes the response.
    /// Implements retry logic with exponential backoff for transient failures.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response into.</typeparam>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">API endpoint path (e.g., "/api/merchants/intents").</param>
    /// <param name="body">Optional request body object (serialized to JSON).</param>
    /// <param name="idempotencyKey">Optional idempotency key for POST requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized response object.</returns>
    public async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendRawAsync(method, path, body, idempotencyKey, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TResponse>(
            JsonSerializerOptionsFactory.Default,
            cancellationToken);
        return result ?? throw new StendlyException("Empty response from API");
    }

    /// <summary>
    /// Sends an HTTP request without deserializing the response body.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">API endpoint path.</param>
    /// <param name="body">Optional request body object (serialized to JSON).</param>
    /// <param name="idempotencyKey">Optional idempotency key for POST requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP response message.</returns>
    public async Task<HttpResponseMessage> SendRawAsync(
        HttpMethod method,
        string path,
        object? body = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var request = new HttpRequestMessage(method, path);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Headers.UserAgent.Add(_userAgent);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-Stendly-SDK", "dotnet");

                // Add idempotency key for mutating requests
                if (idempotencyKey != null)
                {
                    request.Headers.Add("Idempotency-Key", idempotencyKey);
                }

                // Serialize body
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body, JsonSerializerOptionsFactory.Default);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                // Handle rate limiting (429)
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (attempt < _maxRetries)
                    {
                        var delay = CalculateBackoff(attempt);
                        // Check Retry-After header
                        var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds;
                        if (retryAfter.HasValue)
                            delay = (int)retryAfter.Value;

                        response.Dispose();
                        await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
                        continue;
                    }
                    else
                    {
                        await HandleErrorResponseAsync(response);
                    }
                }

                // Client errors (4xx except 429) are not retryable
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    await HandleErrorResponseAsync(response);
                }

                // Server errors (5xx) may be retried
                if ((int)response.StatusCode >= 500)
                {
                    if (attempt < _maxRetries)
                    {
                        var delay = CalculateBackoff(attempt);
                        response.Dispose();
                        await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
                        continue;
                    }
                    else
                    {
                        await HandleErrorResponseAsync(response);
                    }
                }

                // Ensure success for non-error statuses
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (StendlyException)
            {
                // Already handled — rethrow
                throw;
            }
            catch (OperationCanceledException)
            {
                // Propagate cancellation
                throw;
            }
            catch (HttpRequestException ex) when (attempt < _maxRetries)
            {
                lastException = ex;
                var delay = CalculateBackoff(attempt);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new StendlyApiConnectionException(
                    $"Request failed after {_maxRetries + 1} attempts",
                    innerException: ex);
            }
        }

        throw new StendlyApiConnectionException(
            $"Request failed after {_maxRetries + 1} attempts",
            innerException: lastException);
    }

    /// <summary>
    /// Generates a UUID v4 idempotency key.
    /// </summary>
    public static string GenerateIdempotencyKey() => Guid.NewGuid().ToString("D");

    /// <summary>
    /// Calculates exponential backoff with jitter.
    /// </summary>
    private static int CalculateBackoff(int attempt)
    {
        var baseDelay = Math.Pow(2, attempt);
        double jitter;
        lock (_jitter)
        {
            jitter = _jitter.NextDouble() * 0.1 * baseDelay;
        }
        return (int)Math.Min(baseDelay + jitter, 60.0);
    }

    /// <summary>
    /// Parses error response and throws the appropriate exception.
    /// </summary>
    private static async Task HandleErrorResponseAsync(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var requestId = response.Headers.TryGetValues("X-Request-Id", out var values)
            ? values.FirstOrDefault()
            : null;

        string errorMessage;
        Dictionary<string, string>? errorDetails = null;

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            errorMessage = root.TryGetProperty("error", out var errorProp)
                ? errorProp.GetString() ?? $"HTTP {statusCode}"
                : $"HTTP {statusCode}";

            if (root.TryGetProperty("details", out var detailsProp) && detailsProp.ValueKind == JsonValueKind.Object)
            {
                errorDetails = new Dictionary<string, string>();
                foreach (var prop in detailsProp.EnumerateObject())
                {
                    errorDetails[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
            }
        }
        catch
        {
            errorMessage = $"HTTP {statusCode}";
        }

        response.Dispose();

        switch (statusCode)
        {
            case 401:
            case 403:
                throw new StendlyAuthenticationException(errorMessage, statusCode, requestId);
            case 400:
                var field = errorDetails?.GetValueOrDefault("field");
                throw new StendlyValidationException(errorMessage, field, errorDetails, statusCode, requestId);
            case 429:
                int? retryAfter = null;
                if (response.Headers.RetryAfter?.Delta?.TotalSeconds is double seconds)
                    retryAfter = (int)seconds;
                throw new StendlyRateLimitException(errorMessage, retryAfter, statusCode, requestId);
            default:
                throw new StendlyException(errorMessage, statusCode, requestId);
        }
    }

    public void Dispose()
    {
        // We do NOT dispose _httpClient — it was provided externally.
    }
}