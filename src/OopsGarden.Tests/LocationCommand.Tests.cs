using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class LocationCommandTests
{
    [Fact(DisplayName = "Constructor stores location name")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValue()
    {
        var value = new LocationCommand("Kitchen");

        value.Name.Should().Be("Kitchen");
    }
}
