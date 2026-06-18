using Stendly.Exceptions;
using Stendly.Infrastructure;
using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Client for payment intents operations.
/// </summary>
internal sealed class IntentsClient : IIntentsClient
{
    private readonly StendlyHttpClient _http;

    /// <summary>
    /// Initializes a new instance of <see cref="IntentsClient"/>.
    /// </summary>
    /// <param name="http">The HTTP client wrapper.</param>
    public IntentsClient(StendlyHttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <inheritdoc />
    public async Task<PaymentIntent> CreateIntentAsync(
        int amountCents,
        string orderId,
        Guid? terminalId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (amountCents <= 0)
            throw new StendlyValidationException(
                "Amount must be positive.", field: "amountCents");

        if (string.IsNullOrWhiteSpace(orderId))
            throw new StendlyValidationException(
                "Order ID is required.", field: "orderId");

        if (orderId.Length > 100)
            throw new StendlyValidationException(
                "Order ID must not exceed 100 characters.", field: "orderId");

        var key = idempotencyKey ?? StendlyHttpClient.GenerateIdempotencyKey();

        var request = new CreatePaymentIntentRequest
        {
            AmountCents = amountCents,
            OrderId = orderId,
            TerminalId = terminalId,
        };

        return await _http.SendAsync<PaymentIntent>(
            HttpMethod.Post,
            "/api/merchants/intents",
            body: request,
            idempotencyKey: key,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentIntent> RetrieveIntentAsync(
        Guid intentId,
        CancellationToken cancellationToken = default)
    {
        return await _http.SendAsync<PaymentIntent>(
            HttpMethod.Get,
            $"/api/merchants/intents/{intentId:D}",
            cancellationToken: cancellationToken);
    }
}