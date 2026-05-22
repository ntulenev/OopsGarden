
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class AdminUserProjectionTests
{
    [Fact(DisplayName = "Constructor stores admin user projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var value = new AdminUserProjection(id, "User", "user@example.com", false, "en", createdAt, 2);

        value.Id.Should().Be(id);
        value.DisplayName.Should().Be("User");
        value.Email.Should().Be("user@example.com");
        value.IsBlocked.Should().BeFalse();
        value.Language.Should().Be("en");
        value.CreatedAt.Should().Be(createdAt);
        value.Plants.Should().Be(2);
    }
}
