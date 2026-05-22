
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class AdminUserTests
{
    [Fact(DisplayName = "Constructor stores admin user values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var value = new AdminUser(id, "User", "user@example.com", true, "ru", createdAt, 5);

        value.Id.Should().Be(id);
        value.DisplayName.Should().Be("User");
        value.Email.Should().Be("user@example.com");
        value.IsBlocked.Should().BeTrue();
        value.Language.Should().Be("ru");
        value.CreatedAt.Should().Be(createdAt);
        value.Plants.Should().Be(5);
    }
}
