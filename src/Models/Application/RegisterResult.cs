namespace Models.Application;

/// <summary>
/// Represents registration result.
/// </summary>
/// <param name="User">The authenticated user when registration succeeds.</param>
/// <param name="Error">The validation error when registration fails.</param>
public sealed record RegisterResult(AuthenticatedUser? User, RegisterError? Error)
{
    /// <summary>
    /// Gets a value indicating whether registration succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

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
