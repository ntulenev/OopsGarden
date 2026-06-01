using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeleteUserUseCase" />
public sealed class DeleteUserUseCase : IDeleteUserUseCase
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
    public async Task<CommandResult> ExecuteAsync(UserId id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return CommandResult.NotFound;
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return CommandResult.Succeeded;
    }

    private readonly IUnitOfWork _unitOfWork;
}
