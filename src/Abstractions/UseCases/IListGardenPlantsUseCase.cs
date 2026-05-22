namespace Abstractions.UseCases;

/// <summary>
/// Defines garden plant list behavior.
/// </summary>
public interface IListGardenPlantsUseCase
{
    /// <summary>
    /// Lists garden plants for a user.
    /// </summary>
    Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken);
}
