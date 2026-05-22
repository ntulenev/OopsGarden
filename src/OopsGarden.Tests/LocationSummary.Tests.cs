
using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class LocationSummaryTests
{
    [Fact(DisplayName = "Constructor stores location summary values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = LocationId.New();

        var value = new LocationSummary(id, "Kitchen", 4);

        value.Id.Should().Be(id);
        value.Name.Should().Be("Kitchen");
        value.Plants.Should().Be(4);
    }
}
