
using FluentAssertions;

namespace OopsGarden.Tests;

public sealed class LoginCommandTests
{
    [Fact(DisplayName = "Constructor stores credentials")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresCredentials()
    {
        var value = new LoginCommand("user@example.com", "secret");

        value.Email.Should().Be("user@example.com");
        value.Password.Should().Be("secret");
    }
}
