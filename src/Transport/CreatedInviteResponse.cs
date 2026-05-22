namespace Transport;

/// <summary>
/// Represents a created invite.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="Url">The invite URL.</param>
public sealed record CreatedInviteResponse(Guid Id, string Code, string Url);
