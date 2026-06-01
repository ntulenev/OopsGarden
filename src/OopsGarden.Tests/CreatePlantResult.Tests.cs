using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class CreatePlantResultTests
{
    [Fact(DisplayName = "IsSuccess returns true when error is missing")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorIsMissingReturnsTrue()
    {
        var value = CreatePlantResult.Succeeded(Guid.NewGuid());

        value.Status.Should().Be(CommandStatus.Succeeded);
        value.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "IsSuccess returns false when error exists")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorExistsReturnsFalse()
    {
        var value = CreatePlantResult.Invalid(PlantCommandError.InvalidLocation);

        value.Status.Should().Be(CommandStatus.Invalid);
        value.IsSuccess.Should().BeFalse();
        value.ErrorMessage.Should().Be("Invalid location.");
    }
}
