namespace Models.Tests;

public sealed class WateringEventIdTests
{
    [Fact(DisplayName = "From throws when value is empty")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsEmptyThrowsArgumentException() =>
        StrongIdAssertions.FromWhenValueIsEmptyThrowsArgumentException(() => _ = WateringEventId.From(Guid.Empty));

    [Fact(DisplayName = "New creates non-empty value")]
    [Trait("Category", "Unit")]
    public void NewWhenCalledCreatesNonEmptyValue() =>
        StrongIdAssertions.NewWhenCalledCreatesNonEmptyValue(() => WateringEventId.New().Value);
}
