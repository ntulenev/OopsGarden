using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class AuthenticatedUserTests
{
    [Fact(DisplayName = "Constructor stores authenticated user values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();

        var value = new AuthenticatedUser(id, "User", "user@example.com", "ru", "avatar", true);

        value.Id.Should().Be(id);
        value.DisplayName.Should().Be("User");
        value.Email.Should().Be("user@example.com");
        value.Language.Should().Be("ru");
        value.AvatarData.Should().Be("avatar");
        value.IsGardenPublic.Should().BeTrue();
    }
}
