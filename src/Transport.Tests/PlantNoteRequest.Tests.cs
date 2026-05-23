using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantNoteRequestTests
{
    [Fact(DisplayName = "Constructor stores note request values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var request = new PlantNoteRequest("Sprouted", true);

        request.Text.Should().Be("Sprouted");
        request.IsAutomatic.Should().BeTrue();
    }
}
