using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class CreatePlantResultTests
{
    [Fact(DisplayName = "IsSuccess returns true when error is missing")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorIsMissingReturnsTrue()
    {
        var value = new CreatePlantResult(Guid.NewGuid(), null);

        value.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "IsSuccess returns false when error exists")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorExistsReturnsFalse()
    {
        var value = new CreatePlantResult(null, "Invalid location.");

        value.IsSuccess.Should().BeFalse();
    }
}
