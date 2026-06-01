using FluentAssertions;

namespace OopsGarden.Tests;

public sealed class CommandResultTests
{
    [Fact(DisplayName = "Succeeded result reports success")]
    [Trait("Category", "Unit")]
    public void SucceededWhenReadReportsSuccess()
    {
        CommandResult.Succeeded.Status.Should().Be(CommandStatus.Succeeded);
        CommandResult.Succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "NotFound result reports failure")]
    [Trait("Category", "Unit")]
    public void NotFoundWhenReadReportsFailure()
    {
        CommandResult.NotFound.Status.Should().Be(CommandStatus.NotFound);
        CommandResult.NotFound.IsSuccess.Should().BeFalse();
    }
}
