using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class DeleteInviteResultTests
{
    [Fact(DisplayName = "Constructor stores result values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new DeleteInviteResult(DeleteInviteStatus.Invalid, DeleteInviteError.UsedInviteCannotBeDeleted);

        value.Status.Should().Be(DeleteInviteStatus.Invalid);
        value.Error.Should().Be(DeleteInviteError.UsedInviteCannotBeDeleted);
        value.ErrorMessage.Should().Be("Used invite cannot be deleted.");
    }
}
