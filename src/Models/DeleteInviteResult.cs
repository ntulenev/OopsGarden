namespace Models;

/// <summary>
/// Represents invite deletion result.
/// </summary>
/// <param name="Status">The invite deletion status.</param>
/// <param name="Error">The validation error when deletion is not allowed.</param>
public sealed record DeleteInviteResult(DeleteInviteStatus Status, string? Error);
