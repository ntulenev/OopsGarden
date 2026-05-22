using FluentAssertions;

using Models;

using Storage.Repositories;

namespace Storage.Tests;

public sealed class InvitesRepositoryTests
{
    [Fact(DisplayName = "InvitesRepository adds lists and removes invites")]
    [Trait("Category", "Unit")]
    public async Task InvitesRepositoryWhenInviteIsAddedListsAndRemovesInvite()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new InvitesRepository(db);
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));

        // Act
        await repository.AddAsync(invite, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var list = await repository.ListAsync(cancellationToken);
        repository.Remove(invite);
        await db.SaveChangesAsync(cancellationToken);
        var afterRemove = await repository.FindByIdAsync(invite.Id, cancellationToken);

        // Assert
        list.Should().ContainSingle();
        list[0].Code.Should().Be("code");
        afterRemove.Should().BeNull();
    }

    [Fact(DisplayName = "InvitesRepository finds invite by code and id")]
    [Trait("Category", "Unit")]
    public async Task InvitesRepositoryWhenInviteExistsFindsByCodeAndId()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        await using var db = StorageTestContextFactory.CreateDbContext();
        var repository = new InvitesRepository(db);
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        await repository.AddAsync(invite, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Act
        var byCode = await repository.FindByCodeAsync(InviteCode.From("code"), cancellationToken);
        var byId = await repository.FindByIdAsync(invite.Id, cancellationToken);

        // Assert
        byCode.Should().NotBeNull();
        byCode!.Id.Should().Be(invite.Id);
        byId.Should().NotBeNull();
        byId!.Code.Should().Be(invite.Code);
    }
}
