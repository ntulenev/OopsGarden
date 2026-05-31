using Models;

namespace Abstractions.UseCases;

/// <summary>
/// Defines overdue plant reminder list behavior.
/// </summary>
public interface IListOverduePlantRemindersUseCase
{
    /// <summary>
    /// Lists overdue reminders for a plant.
    /// </summary>
    Task<PlantNotesPage?> ExecuteAsync(
        UserId userId,
        PlantId plantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
