using Models;

namespace Logic.UseCases;

/// <summary>
/// Maps authentication domain objects to application models.
/// </summary>
internal static class AuthUseCaseMapping
{
    /// <summary>
    /// Converts an application user to an authenticated user model.
    /// </summary>
    /// <param name="user">The application user.</param>
    /// <returns>The authenticated user model.</returns>
    public static AuthenticatedUser ToAuthenticatedUser(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new AuthenticatedUser(
            user.Id,
            user.DisplayName.Value,
            user.Email.Value,
            user.Language.Value,
            user.AvatarDataUrl?.Value,
            user.IsGardenPublic);
    }
}
