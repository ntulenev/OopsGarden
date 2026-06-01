namespace Models.Application;

/// <summary>
/// Represents registration result.
/// </summary>
/// <param name="Status">The command status.</param>
/// <param name="User">The authenticated user when registration succeeds.</param>
/// <param name="Error">The validation error when registration fails.</param>
public sealed record RegisterResult(CommandStatus Status, AuthenticatedUser? User, RegisterError? Error)
{
    /// <summary>
    /// Gets a value indicating whether registration succeeded.
    /// </summary>
    public bool IsSuccess => Status == CommandStatus.Succeeded;

    /// <summary>
    /// Creates a successful registration result.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <returns>A successful registration result.</returns>
    public static RegisterResult Succeeded(AuthenticatedUser user) =>
        new(CommandStatus.Succeeded, user, null);

    /// <summary>
    /// Creates an invalid registration result.
    /// </summary>
    /// <param name="error">The validation error.</param>
    /// <returns>An invalid registration result.</returns>
    public static RegisterResult Invalid(RegisterError error) =>
        new(CommandStatus.Invalid, null, error);

    /// <summary>
    /// Gets the validation error message when registration fails.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        RegisterError.InvalidInvite => "Invalid invite.",
        RegisterError.EmailAlreadyRegistered => "Email already registered.",
        _ => null
    };
}
