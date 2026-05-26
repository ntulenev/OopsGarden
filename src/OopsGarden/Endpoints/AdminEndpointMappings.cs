using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps administration application models to endpoint responses.
/// </summary>
internal static class AdminEndpointMappings
{
    /// <summary>
    /// Converts an admin invite model to a response.
    /// </summary>
    /// <param name="invite">The admin invite model.</param>
    /// <returns>The admin invite response.</returns>
    public static AdminInviteResponse ToResponse(this AdminInvite invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        return new AdminInviteResponse(
            invite.Id.Value,
            invite.Code,
            invite.CreatedAt,
            invite.CreatedBy,
            invite.UsedAt,
            invite.UsedByUserId?.Value,
            invite.IsRevoked);
    }

    /// <summary>
    /// Converts an admin user model to a response.
    /// </summary>
    /// <param name="user">The admin user model.</param>
    /// <returns>The admin user response.</returns>
    public static AdminUserResponse ToResponse(this AdminUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new AdminUserResponse(
            user.Id.Value,
            user.DisplayName,
            user.Email,
            user.IsBlocked,
            user.Language,
            user.CreatedAt,
            user.Plants);
    }

    /// <summary>
    /// Converts a created invite model to a response.
    /// </summary>
    /// <param name="invite">The created invite model.</param>
    /// <returns>The created invite response.</returns>
    public static CreatedInviteResponse ToResponse(this CreatedInvite invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        return new CreatedInviteResponse(invite.Id.Value, invite.Code, invite.Url.ToString());
    }
}
