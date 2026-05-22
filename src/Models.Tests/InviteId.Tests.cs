namespace Models.Tests;

public sealed class InviteIdTests
{
    [Fact(DisplayName = "From throws when value is empty")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsEmptyThrowsArgumentException() =>
        StrongIdAssertions.FromWhenValueIsEmptyThrowsArgumentException(() => _ = InviteId.From(Guid.Empty));

    [Fact(DisplayName = "New creates non-empty value")]
    [Trait("Category", "Unit")]
    public void NewWhenCalledCreatesNonEmptyValue() =>
        StrongIdAssertions.NewWhenCalledCreatesNonEmptyValue(() => InviteId.New().Value);
}
