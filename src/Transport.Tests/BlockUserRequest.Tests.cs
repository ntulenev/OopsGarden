using FluentAssertions;

namespace Transport.Tests;

public sealed class BlockUserRequestTests
{
    [Fact(DisplayName = "Constructor stores blocked state")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValue()
    {
        var request = new BlockUserRequest(true);

        request.IsBlocked.Should().BeTrue();
    }
}
