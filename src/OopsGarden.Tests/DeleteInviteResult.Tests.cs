using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class DeleteInviteResultTests
{
    [Fact(DisplayName = "Constructor stores result values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new DeleteInviteResult(DeleteInviteStatus.Invalid, "Invite was already used.");

        value.Status.Should().Be(DeleteInviteStatus.Invalid);
        value.Error.Should().Be("Invite was already used.");
    }
}
