using Contracts.Application;

namespace Contracts.Results;

/// <summary>
/// Represents registration result.
/// </summary>
/// <param name="User">The authenticated user when registration succeeds.</param>
/// <param name="Error">The validation error when registration fails.</param>
public sealed record RegisterResult(AuthenticatedUser? User, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether registration succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;
}
