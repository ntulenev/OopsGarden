using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantNoteRequestTests
{
    [Fact(DisplayName = "Constructor stores note request values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var request = new PlantNoteRequest("Sprouted");

        request.Text.Should().Be("Sprouted");
    }
}
