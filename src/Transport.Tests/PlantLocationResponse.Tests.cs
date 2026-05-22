using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantLocationResponseTests
{
    [Fact(DisplayName = "Constructor stores location values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();

        var response = new PlantLocationResponse(id, "Kitchen");

        response.Id.Should().Be(id);
        response.Name.Should().Be("Kitchen");
    }
}
