using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Interface for payment intents operations.
/// </summary>
public interface IIntentsClient
{
    /// <summary>
    /// Creates a new payment intent.
    /// </summary>
    /// <param name="amountCents">Amount to charge in cents (e.g., 4999 = $49.99). Must be positive.</param>
    /// <param name="orderId">Unique order reference (max 100 characters).</param>
    /// <param name="terminalId">Optional terminal ID for POS scenarios.</param>
    /// <param name="idempotencyKey">Optional custom idempotency key. Auto-generated if not provided.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created or existing payment intent.</returns>
    Task<PaymentIntent> CreateIntentAsync(
        int amountCents,
        string orderId,
        Guid? terminalId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a payment intent by ID.
    /// </summary>
    /// <param name="intentId">Payment intent UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment intent with current status.</returns>
    Task<PaymentIntent> RetrieveIntentAsync(
        Guid intentId,
        CancellationToken cancellationToken = default);
}