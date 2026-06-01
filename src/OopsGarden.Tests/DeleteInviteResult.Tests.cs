using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class DeleteInviteResultTests
{
    [Fact(DisplayName = "Constructor stores result values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = DeleteInviteResult.Invalid(DeleteInviteError.UsedInviteCannotBeDeleted);

        value.Status.Should().Be(CommandStatus.Invalid);
        value.Error.Should().Be(DeleteInviteError.UsedInviteCannotBeDeleted);
        value.ErrorMessage.Should().Be("Used invite cannot be deleted.");
    }
}
