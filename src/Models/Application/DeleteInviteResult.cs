namespace Models.Application;

/// <summary>
/// Represents invite deletion result.
/// </summary>
/// <param name="Status">The invite deletion status.</param>
/// <param name="Error">The validation error when deletion is not allowed.</param>
public sealed record DeleteInviteResult(DeleteInviteStatus Status, DeleteInviteError? Error)
{
    /// <summary>
    /// Gets the validation error message when deletion is not allowed.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        DeleteInviteError.UsedInviteCannotBeDeleted => "Used invite cannot be deleted.",
        _ => null
    };
}
