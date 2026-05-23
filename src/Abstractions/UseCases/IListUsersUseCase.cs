namespace Abstractions.UseCases;

/// <summary>
/// Defines admin user list behavior.
/// </summary>
public interface IListUsersUseCase
{
    /// <summary>
    /// Lists users for administration.
    /// </summary>
    Task<IReadOnlyList<AdminUser>> ExecuteAsync(CancellationToken cancellationToken);
}
