using System.Text.Json.Serialization;

namespace Stendly.Models;

/// <summary>
/// Payment intent status enumeration.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentIntentStatus
{
    /// <summary>
    /// Intent created, payment not yet received.
    /// </summary>
    Pending,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Paid,

    /// <summary>
    /// Payment received but amount is less than expected.
    /// </summary>
    Underpaid,

    /// <summary>
    /// Intent expired without payment.
    /// </summary>
    Expired,

    /// <summary>
    /// Intent was cancelled by merchant or system.
    /// </summary>
    Cancelled,
}