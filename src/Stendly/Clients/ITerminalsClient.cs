using Stendly.Models;

namespace Stendly.Clients;

/// <summary>
/// Interface for POS terminals operations.
/// </summary>
public interface ITerminalsClient
{
    /// <summary>
    /// Creates a new POS terminal.
    /// </summary>
    /// <param name="name">Terminal display name (max 100 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created terminal object.</returns>
    Task<Terminal> CreateTerminalAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all terminals for the merchant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of terminal objects, ordered by creation date (newest first).</returns>
    Task<IReadOnlyList<Terminal>> ListTerminalsAsync(
        CancellationToken cancellationToken = default);
}