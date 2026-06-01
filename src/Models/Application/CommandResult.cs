namespace Models.Application;

/// <summary>
/// Represents a generic command execution result.
/// </summary>
/// <param name="Status">The command status.</param>
public sealed record CommandResult(CommandStatus Status)
{
    /// <summary>
    /// Gets a value indicating whether the command succeeded.
    /// </summary>
    public bool IsSuccess => Status == CommandStatus.Succeeded;

    /// <summary>
    /// Gets a successful command result.
    /// </summary>
    public static CommandResult Succeeded { get; } = new(CommandStatus.Succeeded);

    /// <summary>
    /// Gets a not found command result.
    /// </summary>
    public static CommandResult NotFound { get; } = new(CommandStatus.NotFound);
}
