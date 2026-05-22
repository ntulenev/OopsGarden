
using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IDeleteUserUseCase" />
internal sealed class DeleteUserUseCase : IDeleteUserUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeleteUserUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByIdAsync(UserId.From(id), cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
