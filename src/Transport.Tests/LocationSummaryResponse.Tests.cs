using FluentAssertions;

namespace Transport.Tests;

public sealed class LocationSummaryResponseTests
{
    [Fact(DisplayName = "Constructor stores location values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();

        var response = new LocationSummaryResponse(id, "Kitchen", 4);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Kitchen");
        response.Plants.Should().Be(4);
    }
}
