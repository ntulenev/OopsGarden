namespace Abstractions;

/// <summary>
/// Defines registration behavior.
/// </summary>
public interface IRegisterUseCase
{
    /// <summary>
    /// Registers a user by invite.
    /// </summary>
    Task<RegisterResult> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken);
}
