using Abstractions.System;

namespace OopsGarden.Tests;

internal sealed class TestClock : IClock
{
    public TestClock()
        : this(new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero))
    {
    }

    public TestClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
}
