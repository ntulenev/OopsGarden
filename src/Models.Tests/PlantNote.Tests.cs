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
        var note = PlantNote.Create(plantId, text, createdAt: createdAt);

        // Assert
        note.Id.Value.Should().NotBe(Guid.Empty);
        note.PlantId.Should().Be(plantId);
        note.Text.Should().Be(text);
        note.CreatedAt.Should().Be(createdAt);
        note.IsAutomatic.Should().BeFalse();
        note.Reminder.Should().Be(PlantNoteReminder.None);
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

        var reminder = PlantNoteReminder.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date), true);

        // Act
        var note = PlantNote.Restore(id, plantId, text, true, createdAt, reminder);

        // Assert
        note.Id.Should().Be(id);
        note.PlantId.Should().Be(plantId);
        note.Text.Should().Be(text);
        note.CreatedAt.Should().Be(createdAt);
        note.IsAutomatic.Should().BeTrue();
        note.Reminder.Should().Be(reminder);
    }
}
