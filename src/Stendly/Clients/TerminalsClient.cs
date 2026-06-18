using Stendly.Exceptions;
using Stendly.Infrastructure;
using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Client for POS terminals operations.
/// </summary>
internal sealed class TerminalsClient : ITerminalsClient
{
    private readonly StendlyHttpClient _http;

    /// <summary>
    /// Initializes a new instance of <see cref="TerminalsClient"/>.
    /// </summary>
    /// <param name="http">The HTTP client wrapper.</param>
    public TerminalsClient(StendlyHttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <inheritdoc />
    public async Task<Terminal> CreateTerminalAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new StendlyValidationException(
                "Terminal name is required.", field: "name");

        if (name.Length > 100)
            throw new StendlyValidationException(
                "Terminal name must not exceed 100 characters.", field: "name");

        var request = new CreateTerminalRequest { Name = name };

        // Response contains: {"terminal": {...}} — we rely on JsonSerializerOptions
        // to map camelCase properties. We use a wrapper type to extract the nested object.
        using var response = await _http.SendRawAsync(
            HttpMethod.Post,
            "/api/b2b/merchants/terminals",
            body: request,
            cancellationToken: cancellationToken);

        var raw = await System.Text.Json.JsonSerializer.DeserializeAsync<System.Text.Json.JsonElement>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        // Try to extract nested "terminal" object, fallback to top-level
        var terminalElement = raw.TryGetProperty("terminal", out var terminal)
            ? terminal
            : raw;

        return System.Text.Json.JsonSerializer.Deserialize<Terminal>(
            terminalElement.GetRawText(),
            JsonSerializerOptionsFactory.Default)
            ?? throw new StendlyException("Failed to parse terminal response");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Terminal>> ListTerminalsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.SendRawAsync(
            HttpMethod.Get,
            "/api/b2b/merchants/terminals",
            cancellationToken: cancellationToken);

        var raw = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Terminal>>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            JsonSerializerOptionsFactory.Default,
            cancellationToken: cancellationToken);

        return raw?.AsReadOnly() ?? (IReadOnlyList<Terminal>)Array.Empty<Terminal>();
    }
}