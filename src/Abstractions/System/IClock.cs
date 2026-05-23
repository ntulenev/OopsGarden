namespace Abstractions.System;

/// <summary>
/// Provides the current time to application services.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
