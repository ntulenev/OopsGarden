
using FluentAssertions;

namespace OopsGarden.Tests;

public sealed class RegisterCommandTests
{
    [Fact(DisplayName = "Constructor stores registration values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new RegisterCommand("invite", "User", "user@example.com", "secret", "en");

        value.InviteCode.Should().Be("invite");
        value.DisplayName.Should().Be("User");
        value.Email.Should().Be("user@example.com");
        value.Password.Should().Be("secret");
        value.Language.Should().Be("en");
    }
}
