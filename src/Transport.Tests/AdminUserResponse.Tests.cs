using FluentAssertions;

namespace Transport.Tests;

public sealed class AdminUserResponseTests
{
    [Fact(DisplayName = "Constructor stores user values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var response = new AdminUserResponse(userId, "User", "user@example.com", false, "en", createdAt, 2);

        response.Id.Should().Be(userId);
        response.DisplayName.Should().Be("User");
        response.Email.Should().Be("user@example.com");
        response.IsBlocked.Should().BeFalse();
        response.Language.Should().Be("en");
        response.CreatedAt.Should().Be(createdAt);
        response.Plants.Should().Be(2);
    }
}
