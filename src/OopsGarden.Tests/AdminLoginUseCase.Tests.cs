
using FluentAssertions;

using Microsoft.Extensions.Options;

using Logic.Configuration;
using Logic.UseCases;

namespace OopsGarden.Tests;

public sealed class AdminLoginUseCaseTests
{
    [Fact(DisplayName = "Admin login matches configured credentials case-insensitively")]
    [Trait("Category", "Unit")]
    public void AdminLoginWhenCredentialsAreValidReturnsAdmin()
    {
        // Arrange
        var options = Options.Create(new AdminOptions());
        options.Value.Users.Add(new AdminCredential { UserName = "Admin", Password = "secret" });
        var useCase = new AdminLoginUseCase(options);

        // Act
        var result = useCase.Execute(new LoginCommand(" admin ", "secret"));

        // Assert
        result.Should().Be(new AdminLogin("Admin", "Admin"));
    }

    [Fact(DisplayName = "Admin login returns null for invalid credentials")]
    [Trait("Category", "Unit")]
    public void AdminLoginWhenCredentialsAreInvalidReturnsNull()
    {
        // Arrange
        var options = Options.Create(new AdminOptions());
        options.Value.Users.Add(new AdminCredential { UserName = "Admin", Password = "secret" });
        var useCase = new AdminLoginUseCase(options);

        // Act
        var result = useCase.Execute(new LoginCommand("admin", "bad"));

        // Assert
        result.Should().BeNull();
    }
}
