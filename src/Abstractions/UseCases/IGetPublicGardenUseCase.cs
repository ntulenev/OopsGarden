using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines public garden lookup behavior.
/// </summary>
public interface IGetPublicGardenUseCase
{
    /// <summary>
    /// Gets a public garden by owner id.
    /// </summary>
    Task<PublicGarden?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
