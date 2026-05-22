using FluentAssertions;

namespace Transport.Tests;

public sealed class LocationRequestTests
{
    [Fact(DisplayName = "Constructor stores location name")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValue()
    {
        var request = new LocationRequest("Kitchen");

        request.Name.Should().Be("Kitchen");
    }
}
