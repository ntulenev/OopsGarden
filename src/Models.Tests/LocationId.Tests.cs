namespace Models.Tests;

public sealed class LocationIdTests
{
    [Fact(DisplayName = "From throws when value is empty")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsEmptyThrowsArgumentException() =>
        StrongIdAssertions.FromWhenValueIsEmptyThrowsArgumentException(() => _ = LocationId.From(Guid.Empty));

    [Fact(DisplayName = "New creates non-empty value")]
    [Trait("Category", "Unit")]
    public void NewWhenCalledCreatesNonEmptyValue() =>
        StrongIdAssertions.NewWhenCalledCreatesNonEmptyValue(() => LocationId.New().Value);
}
