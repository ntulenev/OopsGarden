using FluentAssertions;

namespace OopsGarden.Tests;

public sealed class CommandStatusTests
{
    [Fact(DisplayName = "Enum exposes command statuses")]
    [Trait("Category", "Unit")]
    public void EnumWhenReadContainsExpectedValues() =>
        Enum.GetValues<CommandStatus>().Should().Contain([
            CommandStatus.Succeeded,
            CommandStatus.NotFound,
            CommandStatus.Invalid
        ]);
}
