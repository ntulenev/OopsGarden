using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantNoteResponseTests
{
    [Fact(DisplayName = "Constructor stores note response values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var response = new PlantNoteResponse(id, "Sprouted", createdAt);

        response.Id.Should().Be(id);
        response.Text.Should().Be("Sprouted");
        response.CreatedAt.Should().Be(createdAt);
    }
}
