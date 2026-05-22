namespace Abstractions;

/// <summary>
/// Defines admin login behavior.
/// </summary>
public interface IAdminLoginUseCase
{
    /// <summary>
    /// Authenticates an administrator.
    /// </summary>
    AdminLogin? Execute(LoginCommand command);
}
