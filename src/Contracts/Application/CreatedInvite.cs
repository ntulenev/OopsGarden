using Models;

namespace Contracts.Application;

/// <summary>
/// Represents a created invite application model.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="Url">The invite URL.</param>
public sealed record CreatedInvite(InviteId Id, string Code, Uri Url);
