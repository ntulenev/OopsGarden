using Models;

namespace Abstractions;

/// <summary>
/// Defines location creation behavior.
/// </summary>
public interface ICreateLocationUseCase
{
    /// <summary>
    /// Creates a garden location.
    /// </summary>
    Task<LocationSummary> ExecuteAsync(UserId userId, LocationCommand command, CancellationToken cancellationToken);
}
