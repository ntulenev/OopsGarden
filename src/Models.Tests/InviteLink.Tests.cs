using FluentAssertions;

namespace Models.Tests;

public sealed class InviteLinkTests
{
    [Fact(DisplayName = "InviteLink create makes usable invite")]
    [Trait("Category", "Unit")]
    public void InviteLinkCreateWhenArgumentsAreValidCreatesUsableInvite()
    {
        // Act
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));

        // Assert
        invite.Id.Value.Should().NotBe(Guid.Empty);
        invite.Code.Value.Should().Be("code");
        invite.CreatedBy.Value.Should().Be("admin");
        invite.CanBeUsed.Should().BeTrue();
        invite.UsedAt.Should().BeNull();
        invite.UsedByUserId.Should().BeNull();
        invite.IsRevoked.Should().BeFalse();
    }

    [Fact(DisplayName = "InviteLink mark used consumes invite")]
    [Trait("Category", "Unit")]
    public void InviteLinkMarkUsedWhenInviteCanBeUsedConsumesInvite()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        var userId = UserId.New();

        // Act
        invite.MarkUsed(userId);

        // Assert
        invite.CanBeUsed.Should().BeFalse();
        invite.UsedAt.Should().NotBeNull();
        invite.UsedByUserId.Should().Be(userId);
    }

    [Fact(DisplayName = "InviteLink mark used throws when invite is revoked")]
    [Trait("Category", "Unit")]
    public void InviteLinkMarkUsedWhenInviteIsRevokedThrowsInvalidOperationException()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.Revoke();

        // Act
        Action act = () => invite.MarkUsed(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "InviteLink revoke throws when invite is used")]
    [Trait("Category", "Unit")]
    public void InviteLinkRevokeWhenInviteIsUsedThrowsInvalidOperationException()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.MarkUsed(UserId.New());

        // Act
        Action act = invite.Revoke;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
