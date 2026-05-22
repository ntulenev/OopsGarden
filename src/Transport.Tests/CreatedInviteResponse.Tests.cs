using FluentAssertions;

namespace Transport.Tests;

public sealed class CreatedInviteResponseTests
{
    [Fact(DisplayName = "Constructor stores created invite values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();

        var response = new CreatedInviteResponse(id, "code", "/?invite=code");

        response.Id.Should().Be(id);
        response.Code.Should().Be("code");
        response.Url.Should().Be("/?invite=code");
    }
}
