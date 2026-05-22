using FluentAssertions;

namespace Transport.Tests;

public sealed class RegisterRequestTests
{
    [Fact(DisplayName = "Constructor stores registration values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var request = new RegisterRequest("invite", "User", "user@example.com", "password", "ru");

        request.InviteCode.Should().Be("invite");
        request.DisplayName.Should().Be("User");
        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("password");
        request.Language.Should().Be("ru");
    }
}
