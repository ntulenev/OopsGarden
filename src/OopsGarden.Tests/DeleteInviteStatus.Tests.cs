using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class DeleteInviteStatusTests
{
    [Fact(DisplayName = "Enum exposes invite deletion statuses")]
    [Trait("Category", "Unit")]
    public void EnumWhenReadContainsExpectedValues() => Enum.GetValues<DeleteInviteStatus>().Should().Contain([DeleteInviteStatus.Deleted, DeleteInviteStatus.NotFound, DeleteInviteStatus.Invalid]);
}
