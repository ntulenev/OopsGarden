
using FluentAssertions;

using Microsoft.AspNetCore.Identity;

using Models;

using Moq;

using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class RegisterUseCaseTests
{
    [Fact(DisplayName = "Register consumes invite and creates user")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsValidCreatesUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin"));
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object, invitesMock.Object);
        var findInviteCalls = 0;
        var emailExistsCalls = 0;
        var addUserCalls = 0;
        var saveCalls = 0;
        AppUser? addedUser = null;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("invite"), cancellationToken))
            .Callback(() => findInviteCalls++)
            .ReturnsAsync(invite);
        usersMock
            .Setup(repo => repo.ExistsByEmailAsync(UserEmail.From("user@example.com"), cancellationToken))
            .Callback(() => emailExistsCalls++)
            .ReturnsAsync(false);
        usersMock
            .Setup(repo => repo.AddAsync(It.IsAny<AppUser>(), cancellationToken))
            .Callback<AppUser, CancellationToken>((user, _) =>
            {
                addUserCalls++;
                addedUser = user;
            })
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "user@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.User.Should().NotBeNull();
        addedUser.Should().NotBeNull();
        invite.CanBeUsed.Should().BeFalse();
        findInviteCalls.Should().Be(1);
        emailExistsCalls.Should().Be(1);
        addUserCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Register returns error when invite is invalid")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsInvalidReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        var findInviteCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("missing"), cancellationToken))
            .Callback(() => findInviteCalls++)
            .ReturnsAsync((InviteLink?)null);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("missing", "User", "user@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid invite.");
        findInviteCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Register returns error when email exists")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenEmailExistsReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin"));
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(usersMock.Object, invitesMock.Object);
        var emailExistsCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("invite"), cancellationToken))
            .ReturnsAsync(invite);
        usersMock
            .Setup(repo => repo.ExistsByEmailAsync(UserEmail.From("USER@example.com"), cancellationToken))
            .Callback(() => emailExistsCalls++)
            .ReturnsAsync(true);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "USER@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already registered.");
        emailExistsCalls.Should().Be(1);
        invite.CanBeUsed.Should().BeTrue();
    }
}
