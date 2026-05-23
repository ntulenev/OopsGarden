using Abstractions.System;

namespace OopsGarden.Startup;

/// <summary>
/// Provides system time.
/// </summary>
internal sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
