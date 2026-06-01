using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class UpdatePlantResultTests
{
    [Fact(DisplayName = "Constructor stores update result values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = UpdatePlantResult.Invalid(PlantCommandError.InvalidLocation);

        value.Status.Should().Be(CommandStatus.Invalid);
        value.Error.Should().Be(PlantCommandError.InvalidLocation);
        value.ErrorMessage.Should().Be("Invalid location.");
    }
}
