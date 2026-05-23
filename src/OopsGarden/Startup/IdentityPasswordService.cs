using Abstractions.Security;

using Microsoft.AspNetCore.Identity;

using Models;

namespace OopsGarden.Startup;

/// <summary>
/// Adapts ASP.NET Core Identity password hashing to application use cases.
/// </summary>
internal sealed class IdentityPasswordService : IPasswordService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityPasswordService"/> class.
    /// </summary>
    public IdentityPasswordService(PasswordHasher<AppUser> hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);
        _hasher = hasher;
    }

    /// <inheritdoc />
    public PasswordHash HashPassword(AppUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        return PasswordHash.From(_hasher.HashPassword(user, password));
    }

    /// <inheritdoc />
    public bool VerifyPassword(AppUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        return _hasher.VerifyHashedPassword(user, user.PasswordHash.Value, password) != PasswordVerificationResult.Failed;
    }

    private readonly PasswordHasher<AppUser> _hasher;
}
