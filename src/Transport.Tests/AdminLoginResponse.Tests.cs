using FluentAssertions;

namespace Transport.Tests;

public sealed class AdminLoginResponseTests
{
    [Fact(DisplayName = "Constructor stores admin values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var response = new AdminLoginResponse("admin", "Admin");

        response.Name.Should().Be("admin");
        response.Role.Should().Be("Admin");
    }
}
