
namespace Logic.UseCases;

/// <summary>
/// Represents the result of resolving a requested location id.
/// </summary>
/// <param name="LocationId">The resolved location id.</param>
/// <param name="Error">The validation error when the location cannot be resolved.</param>
internal sealed record ResolveLocationResult(LocationId? LocationId, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether the location was resolved successfully.
    /// </summary>
    public bool IsSuccess => Error is null;
}
