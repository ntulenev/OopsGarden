using FluentAssertions;

namespace Transport.Tests;

public sealed class AuthenticatedUserResponseTests
{
    [Fact(DisplayName = "Constructor stores user values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();

        var response = new AuthenticatedUserResponse(id, "User", "user@example.com", "en", "data:image/png;base64,abc", true);

        response.Id.Should().Be(id);
        response.DisplayName.Should().Be("User");
        response.Email.Should().Be("user@example.com");
        response.Language.Should().Be("en");
        response.AvatarDataUrl.Should().Be("data:image/png;base64,abc");
        response.IsGardenPublic.Should().BeTrue();
    }
}
