using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class AdminInviteTests
{
    [Fact(DisplayName = "Constructor stores invite values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = InviteId.New();
        var userId = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;
        var usedAt = createdAt.AddMinutes(1);

        var value = new AdminInvite(id, "code", createdAt, "admin", usedAt, userId, true);

        value.Id.Should().Be(id);
        value.Code.Should().Be("code");
        value.CreatedAt.Should().Be(createdAt);
        value.CreatedBy.Should().Be("admin");
        value.UsedAt.Should().Be(usedAt);
        value.UsedByUserId.Should().Be(userId);
        value.IsRevoked.Should().BeTrue();
    }
}
