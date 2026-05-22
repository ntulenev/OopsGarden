using Models;

namespace Abstractions;

/// <summary>
/// Defines persistence operations for application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by email.
    /// </summary>
    Task<AppUser?> FindByEmailAsync(UserEmail email, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a user by id.
    /// </summary>
    Task<AppUser?> FindByIdAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a user with the specified email exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    Task AddAsync(AppUser user, CancellationToken cancellationToken);

    /// <summary>
    /// Lists users for administration.
    /// </summary>
    Task<IReadOnlyList<AdminUserProjection>> ListAdminUsersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Removes a user.
    /// </summary>
    void Remove(AppUser user);
}
