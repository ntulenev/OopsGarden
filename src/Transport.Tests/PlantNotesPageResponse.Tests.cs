using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantNotesPageResponseTests
{
    [Fact(DisplayName = "Constructor stores note page values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var item = new PlantNoteResponse(Guid.NewGuid(), "Sprouted", DateTimeOffset.UtcNow);

        var response = new PlantNotesPageResponse([item], 2, 5, 12, true, true);

        response.Items.Should().ContainSingle().Which.Should().Be(item);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.Total.Should().Be(12);
        response.HasPrevious.Should().BeTrue();
        response.HasNext.Should().BeTrue();
    }
}
