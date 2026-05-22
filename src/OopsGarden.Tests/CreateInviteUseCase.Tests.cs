using System.Security.Claims;

using Abstractions.Repositories;

using FluentAssertions;

using Logic.UseCases;

using Models;

using Moq;

namespace OopsGarden.Tests;

public sealed class CreateInviteUseCaseTests
{
    [Fact(DisplayName = "ExecuteAsync creates invite for current admin")]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsyncWhenPrincipalHasNameCreatesInvite()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "Test"));
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = TestUnitOfWorkFactory.Create(invites: invitesMock.Object);
        var addCalls = 0;
        var saveCalls = 0;
        InviteLink? addedInvite = null;

        invitesMock
            .Setup(repo => repo.AddAsync(It.IsAny<InviteLink>(), cancellationToken))
            .Callback<InviteLink, CancellationToken>((invite, _) =>
            {
                addCalls++;
                addedInvite = invite;
            })
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreateInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(principal, cancellationToken);

        // Assert
        result.Code.Should().NotBeNullOrWhiteSpace();
        result.Id.Should().Be(addedInvite!.Id);
        addedInvite.CreatedBy.Value.Should().Be("admin");
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }
}
