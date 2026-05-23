using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines location rename behavior.
/// </summary>
public interface IRenameLocationUseCase
{
    /// <summary>
    /// Renames a garden location.
    /// </summary>
    Task<LocationSummary?> ExecuteAsync(UserId userId, LocationId id, LocationCommand command, CancellationToken cancellationToken);
}
