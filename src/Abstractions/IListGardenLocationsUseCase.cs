using Models;

namespace Abstractions;

/// <summary>
/// Defines garden location list behavior.
/// </summary>
public interface IListGardenLocationsUseCase
{
    /// <summary>
    /// Lists garden locations for a user.
    /// </summary>
    Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken);
}
