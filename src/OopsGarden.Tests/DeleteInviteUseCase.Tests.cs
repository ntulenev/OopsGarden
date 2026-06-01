using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class DeleteInviteUseCaseTests
{
    [Fact(DisplayName = "Delete invite returns invalid when invite is used")]
    [Trait("Category", "Unit")]
    public async Task DeleteInviteWhenInviteIsUsedReturnsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.MarkUsed(UserId.New());
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        invitesMock.Setup(repo => repo.FindByIdAsync(invite.Id, cancellationToken)).ReturnsAsync(invite);
        var useCase = new DeleteInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(invite.Id, cancellationToken);

        // Assert
        result.Status.Should().Be(CommandStatus.Invalid);
        result.Error.Should().Be(DeleteInviteError.UsedInviteCannotBeDeleted);
        result.ErrorMessage.Should().Be("Used invite cannot be deleted.");
    }

    [Fact(DisplayName = "ExecuteAsync deletes existing unused invite")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenInviteIsUnusedDeletesInvite()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        var findCalls = 0;
        var removeCalls = 0;
        var saveCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByIdAsync(invite.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(invite);
        invitesMock
            .Setup(repo => repo.Remove(invite))
            .Callback(() => removeCalls++);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeleteInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(invite.Id, cancellationToken);

        // Assert
        result.Status.Should().Be(CommandStatus.Succeeded);
        findCalls.Should().Be(1);
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
