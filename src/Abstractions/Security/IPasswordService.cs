using Models;

namespace Abstractions.Security;

/// <summary>
/// Provides password hashing operations for application use cases.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password for the specified user.
    /// </summary>
    PasswordHash HashPassword(AppUser user, string password);

    /// <summary>
    /// Checks whether a password matches the stored hash for the specified user.
    /// </summary>
    bool VerifyPassword(AppUser user, string password);
}
