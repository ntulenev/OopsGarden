namespace Abstractions.UseCases;

/// <summary>
/// Defines settings update behavior.
/// </summary>
public interface IUpdateSettingsUseCase
{
    /// <summary>
    /// Updates user settings.
    /// </summary>
    Task<AuthenticatedUser?> ExecuteAsync(UserId userId, SettingsCommand command, CancellationToken cancellationToken);
}
