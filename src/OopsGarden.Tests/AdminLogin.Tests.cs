using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class AdminLoginTests
{
    [Fact(DisplayName = "Constructor stores admin login values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new AdminLogin("admin", "Admin");

        value.Name.Should().Be("admin");
        value.Role.Should().Be("Admin");
    }
}
