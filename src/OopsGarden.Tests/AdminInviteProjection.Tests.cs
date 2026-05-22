
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class AdminInviteProjectionTests
{
    [Fact(DisplayName = "Constructor stores invite projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = InviteId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var value = new AdminInviteProjection(id, "code", createdAt, "admin", null, null, false);

        value.Id.Should().Be(id);
        value.Code.Should().Be("code");
        value.CreatedAt.Should().Be(createdAt);
        value.CreatedBy.Should().Be("admin");
        value.UsedAt.Should().BeNull();
        value.UsedByUserId.Should().BeNull();
        value.IsRevoked.Should().BeFalse();
    }
}
