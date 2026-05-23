using FluentAssertions;

namespace Models.Tests;

public sealed class PlantNoteTests
{
    [Fact(DisplayName = "Plant note create sets values")]
    [Trait("Category", "Unit")]
    public void PlantNoteCreateWhenArgumentsAreValidSetsValues()
    {
        // Arrange
        var plantId = PlantId.New();
        var text = PlantNoteText.From("Sprouted");
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var note = PlantNote.Create(plantId, text, createdAt);

        // Assert
        note.Id.Value.Should().NotBe(Guid.Empty);
        note.PlantId.Should().Be(plantId);
        note.Text.Should().Be(text);
        note.CreatedAt.Should().Be(createdAt);
    }

    [Fact(DisplayName = "Plant note restore rehydrates persisted values")]
    [Trait("Category", "Unit")]
    public void PlantNoteRestoreWhenArgumentsAreValidCreatesNote()
    {
        // Arrange
        var id = PlantNoteId.New();
        var plantId = PlantId.New();
        var text = PlantNoteText.From("Repotted");
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var note = PlantNote.Restore(id, plantId, text, createdAt);

        // Assert
        note.Id.Should().Be(id);
        note.PlantId.Should().Be(plantId);
        note.Text.Should().Be(text);
        note.CreatedAt.Should().Be(createdAt);
    }
}
