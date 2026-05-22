using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class UpdatePlantResultTests
{
    [Fact(DisplayName = "Constructor stores update result values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new UpdatePlantResult(UpdatePlantStatus.NotFound, "Plant was not found.");

        value.Status.Should().Be(UpdatePlantStatus.NotFound);
        value.Error.Should().Be("Plant was not found.");
    }
}
