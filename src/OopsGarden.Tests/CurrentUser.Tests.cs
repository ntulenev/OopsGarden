using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class CurrentUserTests
{
    [Fact(DisplayName = "Constructor stores current user values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();

        var value = new CurrentUser(true, id, "User", "User", "ru", "avatar", true);

        value.Authenticated.Should().BeTrue();
        value.Id.Should().Be(id);
        value.Name.Should().Be("User");
        value.Role.Should().Be("User");
        value.Language.Should().Be("ru");
        value.AvatarData.Should().Be("avatar");
        value.IsGardenPublic.Should().BeTrue();
    }
}
