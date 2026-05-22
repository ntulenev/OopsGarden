using Abstractions;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IListUsersUseCase" />
internal sealed class ListUsersUseCase : IListUsersUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListUsersUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListUsersUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminUser>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.ListAdminUsersAsync(cancellationToken).ConfigureAwait(false);
        return [.. users
            .Select(user => new AdminUser(
                user.Id,
                user.DisplayName,
                user.Email,
                user.IsBlocked,
                user.Language,
                user.CreatedAt,
                user.Plants))];
    }

    private readonly IUnitOfWork _unitOfWork;
}
