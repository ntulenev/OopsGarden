namespace Models.Tests;

public sealed class PlantIdTests
{
    [Fact(DisplayName = "From throws when value is empty")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsEmptyThrowsArgumentException() =>
        StrongIdAssertions.FromWhenValueIsEmptyThrowsArgumentException(() => _ = PlantId.From(Guid.Empty));

    [Fact(DisplayName = "New creates non-empty value")]
    [Trait("Category", "Unit")]
    public void NewWhenCalledCreatesNonEmptyValue() =>
        StrongIdAssertions.NewWhenCalledCreatesNonEmptyValue(() => PlantId.New().Value);
}
