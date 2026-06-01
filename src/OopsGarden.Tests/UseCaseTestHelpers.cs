using System.Security.Claims;

using Abstractions.Repositories;

using Models;

using Moq;

namespace OopsGarden.Tests;

internal static class TestUsers
{
    public static AppUser Create(string email) =>
        AppUser.Create(
            UserEmail.From(email),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"));

    public static ClaimsPrincipal CreatePrincipal(UserId userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Name, "User"),
                new Claim(ClaimTypes.Role, role)
            ],
            "Test"));
}

internal static class TestUnitOfWorkFactory
{
    public static Mock<IUnitOfWork> Create(
        IUserRepository? users = null,
        IInviteRepository? invites = null,
        IPlantRepository? plants = null,
        ILocationRepository? locations = null)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        if (users is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Users).Returns(users);
        }

        if (invites is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Invites).Returns(invites);
        }

        if (plants is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Plants).Returns(plants);
        }

        if (locations is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Locations).Returns(locations);
        }

        return unitOfWorkMock;
    }
}
