using Abstractions.Security;

using Microsoft.AspNetCore.Identity;

using Models;

namespace OopsGarden.Tests;

internal sealed class TestPasswordService : IPasswordService
{
    public PasswordHash HashPassword(AppUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        return PasswordHash.From(_hasher.HashPassword(user, password));
    }

    public bool VerifyPassword(AppUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        return _hasher.VerifyHashedPassword(user, user.PasswordHash.Value, password) != PasswordVerificationResult.Failed;
    }

    private readonly PasswordHasher<AppUser> _hasher = new();
}
