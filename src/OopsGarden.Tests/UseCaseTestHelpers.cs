using System.Security.Claims;



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
        IGardenRepository? garden = null)
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

        if (garden is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Garden).Returns(garden);
        }

        return unitOfWorkMock;
    }
}
