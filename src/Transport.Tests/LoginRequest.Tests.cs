using FluentAssertions;

namespace Transport.Tests;

public sealed class LoginRequestTests
{
    [Fact(DisplayName = "Constructor stores credentials")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresCredentials()
    {
        var request = new LoginRequest("user@example.com", "password");

        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("password");
    }
}
