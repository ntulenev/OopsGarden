namespace Models.Application;

/// <summary>
/// Represents invite deletion result.
/// </summary>
/// <param name="Status">The invite deletion status.</param>
/// <param name="Error">The validation error when deletion is not allowed.</param>
public sealed record DeleteInviteResult(CommandStatus Status, DeleteInviteError? Error)
{
    /// <summary>
    /// Gets the validation error message when deletion is not allowed.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        DeleteInviteError.UsedInviteCannotBeDeleted => "Used invite cannot be deleted.",
        _ => null
    };

    /// <summary>
    /// Gets a successful invite deletion result.
    /// </summary>
    public static DeleteInviteResult Succeeded { get; } = new(CommandStatus.Succeeded, null);

    /// <summary>
    /// Gets a not found invite deletion result.
    /// </summary>
    public static DeleteInviteResult NotFound { get; } = new(CommandStatus.NotFound, null);

    /// <summary>
    /// Creates an invalid invite deletion result.
    /// </summary>
    public static DeleteInviteResult Invalid(DeleteInviteError error) => new(CommandStatus.Invalid, error);
}
