using FluentAssertions;

namespace Transport.Tests;

public sealed class AdminInviteResponseTests
{
    [Fact(DisplayName = "Constructor stores invite values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var inviteId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var response = new AdminInviteResponse(inviteId, "code", createdAt, "admin", null, null, false);

        response.Id.Should().Be(inviteId);
        response.Code.Should().Be("code");
        response.CreatedAt.Should().Be(createdAt);
        response.CreatedBy.Should().Be("admin");
        response.UsedAt.Should().BeNull();
        response.UsedByUserId.Should().BeNull();
        response.IsRevoked.Should().BeFalse();
    }
}
