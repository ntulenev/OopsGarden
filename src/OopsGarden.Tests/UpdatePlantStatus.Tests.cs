
using FluentAssertions;

namespace OopsGarden.Tests;

public sealed class UpdatePlantStatusTests
{
    [Fact(DisplayName = "Enum exposes plant update statuses")]
    [Trait("Category", "Unit")]
    public void EnumWhenReadContainsExpectedValues()
    {
        Enum.GetValues<UpdatePlantStatus>().Should().Contain([UpdatePlantStatus.Updated, UpdatePlantStatus.NotFound, UpdatePlantStatus.Invalid]);
    }
}
